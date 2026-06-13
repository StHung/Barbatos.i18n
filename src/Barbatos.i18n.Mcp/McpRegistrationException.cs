// This Source Code Form is subject to the terms of the MIT License.
// If a copy of the MIT was not distributed with this file, You can obtain one at https://opensource.org/licenses/MIT.
// Copyright (C) Pham The Hung and Barbatos.i18n Contributors.
// All Rights Reserved.

namespace Barbatos.i18n.Mcp;

/// <summary>
/// Represents errors that occur during Model Context Protocol (MCP) registration.
/// </summary>
public class McpRegistrationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="McpRegistrationException"/> class.
    /// </summary>
    public McpRegistrationException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="McpRegistrationException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public McpRegistrationException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="McpRegistrationException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    public McpRegistrationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
