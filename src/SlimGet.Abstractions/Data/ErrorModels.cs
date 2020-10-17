// This file is a part of SlimGet project.
//
// Copyright 2019 Emzi0767
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;

namespace SlimGet.Data
{
    /// <summary>
    /// Simplified error model, for non-debugging purposes.
    /// </summary>
    public class SimpleErrorModel
    {
        /// <summary>
        /// Gets the ID of the request.
        /// </summary>
        public string RequestId { get; }

        /// <summary>
        /// Creates a new error model.
        /// </summary>
        /// <param name="reqId">ID of the request.</param>
        public SimpleErrorModel(string reqId)
        {
            this.RequestId = reqId;
        }
    }

    /// <summary>
    /// Detailed error model, for use in development or debugging scenarios only. Should not be used in production.
    /// </summary>
    public sealed class DeveloperErrorModel : SimpleErrorModel
    {
        /// <summary>
        /// Gets the path at which the error occured.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the details of the exception that occured.
        /// </summary>
        public ExceptionModel Exception { get; }

        /// <summary>
        /// Creates a new detailed exception model.
        /// </summary>
        /// <param name="reqId">ID of the request.</param>
        /// <param name="path">Path where the exception occured.</param>
        /// <param name="ex">Exception thrown by the handler.</param>
        public DeveloperErrorModel(string reqId, string path, Exception ex)
            : base(reqId)
        {
            this.Path = path;

            if (ex != null)
                this.Exception = new ExceptionModel(ex);
        }
    }

    /// <summary>
    /// Contains full detail of the exception that occured.
    /// </summary>
    public sealed class ExceptionModel
    {
        /// <summary>
        /// Gets the type of the exception.
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Gets the message the exception provided.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the stack trace of the exception.
        /// </summary>
        public string StackTrace { get; }

        /// <summary>
        /// Gets the nested exception, if applicable.
        /// </summary>
        public ExceptionModel InnerException { get; }

        /// <summary>
        /// Creates a new exception model from a given exception.
        /// </summary>
        /// <param name="ex">Exception to create a model from.</param>
        public ExceptionModel(Exception ex)
        {
            if (ex == null)
                throw new ArgumentNullException(nameof(ex), "Exception cannot be null.");

            this.Type = ex.GetType().ToString();
            this.Message = ex.Message;
            this.StackTrace = ex.StackTrace;

            if (ex.InnerException != null)
                this.InnerException = new ExceptionModel(ex.InnerException);
        }
    }
}
