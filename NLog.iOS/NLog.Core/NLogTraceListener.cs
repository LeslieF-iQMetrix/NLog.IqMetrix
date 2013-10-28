// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Xml;
using NLog.Core.Time;

#if !SILVERLIGHT

namespace NLog.Core
{
    /// <summary>
    /// TraceListener which routes all messages through NLog.
    /// </summary>
    public class NLogTraceListener : TraceListener
    {
        private static readonly Assembly systemAssembly = typeof(Trace).Assembly;
        private LogFactory logFactory;
        private LogLevel defaultLogLevel = LogLevel.Debug;
        private bool attributesLoaded;
#if !NET_CF
        private bool autoLoggerName;
#endif
        private LogLevel forceLogLevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="NLogTraceListener"/> class.
        /// </summary>
        public NLogTraceListener()
        {
        }

        /// <summary>
        /// Gets or sets the log factory to use when outputting messages (null - use LogManager).
        /// </summary>
        public LogFactory LogFactory
        {
            get
            {
                this.InitAttributes();
                return this.logFactory;
            }

            set
            {
                this.attributesLoaded = true;
                this.logFactory = value;
            }
        }

        /// <summary>
        /// Gets or sets the default log level.
        /// </summary>
        public LogLevel DefaultLogLevel
        {
            get
            {
                this.InitAttributes();
                return this.defaultLogLevel;
            }

            set
            {
                this.attributesLoaded = true;
                this.defaultLogLevel = value;
            }
        }

        /// <summary>
        /// Gets or sets the log which should be always used regardless of source level.
        /// </summary>
        public LogLevel ForceLogLevel
        {
            get
            {
                this.InitAttributes();
                return this.forceLogLevel;
            }

            set
            {
                this.attributesLoaded = true;
                this.forceLogLevel = value;
            }
        }

#if !NET_CF
        /// <summary>
        /// Gets a value indicating whether the trace listener is thread safe.
        /// </summary>
        /// <value></value>
        /// <returns>true if the trace listener is thread safe; otherwise, false. The default is false.</returns>
        public override bool IsThreadSafe
        {
            get { return true; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use auto logger name detected from the stack trace.
        /// </summary>
        public bool AutoLoggerName
        {
            get
            {
                this.InitAttributes();
                return this.autoLoggerName;
            }

            set
            {
                this.attributesLoaded = true;
                this.autoLoggerName = value;
            }
        }
#endif

        /// <summary>
        /// When overridden in a derived class, writes the specified message to the listener you create in the derived class.
        /// </summary>
        /// <param name="message">A message to write.</param>
        public override void Write(string message)
        {
#if !NET_CF
            this.ProcessLogEventInfo(this.DefaultLogLevel, null, message, null, null, TraceEventType.Resume, null);
#else
            this.ProcessLogEventInfo(this.DefaultLogLevel, null, message, null, null);
#endif
        }

        /// <summary>
        /// When overridden in a derived class, writes a message to the listener you create in the derived class, followed by a line terminator.
        /// </summary>
        /// <param name="message">A message to write.</param>
        public override void WriteLine(string message)
        {
#if !NET_CF
            this.ProcessLogEventInfo(this.DefaultLogLevel, null, message, null, null, TraceEventType.Resume, null);
#else
            this.ProcessLogEventInfo(this.DefaultLogLevel, null, message, null, null);
#endif
        }

        /// <summary>
        /// When overridden in a derived class, closes the output stream so it no longer receives tracing or debugging output.
        /// </summary>
        public override void Close()
        {
        }

        /// <summary>
        /// Emits an error message.
        /// </summary>
        /// <param name="message">A message to emit.</param>
        public override void Fail(string message)
        {
#if !NET_CF
            this.ProcessLogEventInfo(LogLevel.Error, null, message, null, null, TraceEventType.Error, null);
#else
            this.ProcessLogEventInfo(LogLevel.Error, null, message, null, null);
#endif
        }

        /// <summary>
        /// Emits an error message and a detailed error message.
        /// </summary>
        /// <param name="message">A message to emit.</param>
        /// <param name="detailMessage">A detailed message to emit.</param>
        public override void Fail(string message, string detailMessage)
        {
#if !NET_CF
            this.ProcessLogEventInfo(LogLevel.Error, null, message + " " + detailMessage, null, null, TraceEventType.Error, null);
#else
            this.ProcessLogEventInfo(LogLevel.Error, null, message + " " + detailMessage, null, null);
#endif
        }

        /// <summary>
        /// Flushes the output buffer.
        /// </summary>
        public override void Flush()
        {
            if (this.LogFactory != null)
            {
                this.LogFactory.Flush();
            }
            else
            {
                LogManager.Flush();
            }
        }

#if !NET_CF
        private void ProcessLogEventInfo(LogLevel logLevel, string loggerName, [Localizable(false)] string message, object[] arguments, int? eventId, TraceEventType? eventType, Guid? relatedActiviyId)
#else
        private void ProcessLogEventInfo(LogLevel logLevel, string loggerName, [Localizable(false)] string message, object[] arguments, int? eventId)
#endif
        {
            var ev = new LogEventInfo();

            ev.LoggerName = (loggerName ?? this.Name) ?? string.Empty;
            
#if !NET_CF
            if (this.AutoLoggerName)
            {
                var stack = new StackTrace();
                int userFrameIndex = -1;
                MethodBase userMethod = null;

                for (int i = 0; i < stack.FrameCount; ++i)
                {
                    var frame = stack.GetFrame(i);
                    var method = frame.GetMethod();

                    if (method.DeclaringType == this.GetType())
                    {
                        // skip all methods of this type
                        continue;
                    }

                    if (method.DeclaringType.Assembly == systemAssembly)
                    {
                        // skip all methods from System.dll
                        continue;
                    }

                    userFrameIndex = i;
                    userMethod = method;
                    break;
                }

                if (userFrameIndex >= 0)
                {
                    ev.SetStackTrace(stack, userFrameIndex);
                    if (userMethod.DeclaringType != null)
                    {
                        ev.LoggerName = userMethod.DeclaringType.FullName;
                    }
                }
            }

            if (eventType.HasValue)
            {
                ev.Properties.Add("EventType", eventType.Value);
            }

            if (relatedActiviyId.HasValue)
            {
                ev.Properties.Add("RelatedActivityID", relatedActiviyId.Value);
            }
#endif

            ev.TimeStamp = TimeSource.Current.Time;
            ev.Message = message;
            ev.Parameters = arguments;
            ev.Level = this.forceLogLevel ?? logLevel;

            if (eventId.HasValue)
            {
                ev.Properties.Add("EventID", eventId.Value);
            }

            Logger logger;
            if (this.LogFactory != null)
            {
                logger = this.LogFactory.GetLogger(ev.LoggerName);
            }
            else
            {
                logger = LogManager.GetLogger(ev.LoggerName);
            }

            logger.Log(ev);
        }

        private void InitAttributes()
        {
            if (!this.attributesLoaded)
            {
                this.attributesLoaded = true;
#if !NET_CF
                foreach (DictionaryEntry de in this.Attributes)
                {
                    var key = (string)de.Key;
                    var value = (string)de.Value;

                    switch (key.ToUpperInvariant())
                    {
                        case "DEFAULTLOGLEVEL":
                            this.defaultLogLevel = LogLevel.FromString(value);
                            break;

                        case "FORCELOGLEVEL":
                            this.forceLogLevel = LogLevel.FromString(value);
                            break;

                        case "AUTOLOGGERNAME":
                            this.AutoLoggerName = XmlConvert.ToBoolean(value);
                            break;
                    }
                }
#endif
            }
        }
    }
}

#endif