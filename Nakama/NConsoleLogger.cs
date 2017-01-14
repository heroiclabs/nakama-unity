/**
 * Copyright 2017 The Nakama Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

namespace Nakama
{
    public class NConsoleLogger : INLogger
    {
        public void Trace(object message)
        {
            writeInternal(ConsoleColor.DarkGray, message.ToString());
        }

        public void Trace(object message, Exception exception)
        {
            writeInternal(ConsoleColor.DarkGray, "{0}, Exception={1}", message, exception);
        }

        public void TraceIf(bool condition, object message)
        {
            if (condition) Trace(message);
        }

        public void TraceIf(bool condition, object message, Exception exception)
        {
            if (condition) Trace(message, exception);
        }

        public void TraceFormat(string format, params object[] args)
        {
            writeInternal(ConsoleColor.DarkGray, format, args);
        }

        public void TraceFormatIf(bool condition, string format, params object[] args)
        {
            if (condition) TraceFormat(format, args);
        }

        public void Debug(object message)
        {
            writeInternal(ConsoleColor.Gray, message.ToString());
        }

        public void Debug(object message, Exception exception)
        {
            writeInternal(ConsoleColor.Gray, "{0}, Exception={1}", message, exception);
        }

        public void DebugFormat(string format, params object[] args)
        {
            writeInternal(ConsoleColor.Gray, format, args);
        }

        public void Info(object message)
        {
            writeInternal(ConsoleColor.White, message.ToString());
        }

        public void Info(object message, Exception exception)
        {
            writeInternal(ConsoleColor.White, "{0}, Exception={1}", message, exception);
        }

        public void InfoFormat(string format, params object[] args)
        {
            writeInternal(ConsoleColor.White, format, args);
        }

        public void Warn(object message)
        {
            writeInternal(ConsoleColor.Magenta, message.ToString());
        }

        public void Warn(object message, Exception exception)
        {
            writeInternal(ConsoleColor.Magenta, "{0}, Exception={1}", message, exception);
        }

        public void WarnFormat(string format, params object[] args)
        {
            writeInternal(ConsoleColor.Magenta, format, args);
        }

        public void Error(object message)
        {
            writeInternal(ConsoleColor.Yellow, message.ToString());
        }

        public void Error(object message, Exception exception)
        {
            writeInternal(ConsoleColor.Yellow, "{0}, Exception={1}", message, exception);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            writeInternal(ConsoleColor.Yellow, format, args);
        }

        public void Fatal(object message)
        {
            writeInternal(ConsoleColor.Red, message.ToString());
        }

        public void Fatal(object message, Exception exception)
        {
            writeInternal(ConsoleColor.Red, "{0}, Exception={1}", message, exception);
        }

        public void FatalFormat(string format, params object[] args)
        {
            writeInternal(ConsoleColor.Red, format, args);
        }

        private void writeInternal(ConsoleColor color, string format, params object[] args)
        {
            Console.Out.WriteLine(format, args);
        }
    }
}
