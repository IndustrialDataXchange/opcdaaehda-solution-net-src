#region Copyright (c) 2011-2021 Technosoftware GmbH. All rights reserved
//-----------------------------------------------------------------------------
// Copyright (c) 2011-2021 Technosoftware GmbH. All rights reserved
// Web: https://technosoftware.com  
//
// The Software is subject to the Technosoftware GmbH Source Code License Agreement, 
// which can be found here:
// https://technosoftware.com/documents/Source_License_Agreement.pdf
//-----------------------------------------------------------------------------
#endregion Copyright (c) 2011-2021 Technosoftware GmbH. All rights reserved

#region Using Directives
using System;
using System.Linq;
using static System.String;

using Technosoftware.DaAeHdaClient.Licensing;
using Technosoftware.DaAeHdaClient.Licensing.Validation;
#endregion

namespace Technosoftware.DaAeHdaClient
{
    /// <summary>
    /// Manages the license to enable the different product versions.
    /// </summary>
    public class ApplicationInstance
    {
        #region Nested Enums
        /// <summary>
        /// The possible authentication levels.
        /// </summary>
        [Flags]
        public enum AuthenticationLevel : uint
        {
            /// <summary>
            /// Tells DCOM to choose the authentication level using its normal security blanket negotiation algorithm.
            /// </summary>
            Default = 0,

            /// <summary>
            /// Performs no authentication.
            /// </summary>
            None = 1,

            /// <summary>
            /// Authenticates the credentials of the client only when the client establishes a relationship with the server. Datagram transports always use Packet instead.
            /// </summary>
            Connect = 2,

            /// <summary>
            /// Authenticates only at the beginning of each remote procedure call when the server receives the request. Datagram transports use Packet instead.
            /// </summary>
            Call = 3,

            /// <summary>
            /// Authenticates that all data received is from the expected client.
            /// </summary>
            Packet = 4,

            /// <summary>
            /// Authenticates and verifies that none of the data transferred between client and server has been modified.
            /// </summary>
            Integrity = 5,

            /// <summary>
            /// Authenticates all previous levels and encrypts the argument value of each remote procedure call.
            /// </summary>
            Privacy = 6,
        }
        #endregion

        #region Properties
        /// <summary>
        /// This flag suppresses the conversion to local time done during marshalling.
        /// </summary>
        public bool TimeAsUtc
        {
            get => Com.Interop.PreserveUtc;
            set => Com.Interop.PreserveUtc = value;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Initializes COM security. This should be called directly at the beginning of an application and can only be called once.
        /// </summary>
        /// <param name="authenticationLevel">The default authentication level for the process. Both servers and clients use this parameter when they call CoInitializeSecurity. With the Windows Update KB5004442 a higher authentication level of Integrity must be used.</param>
        public static void InitializeSecurity(AuthenticationLevel authenticationLevel)
        {
            if (!InitializeSecurityCalled)
            {
                Com.Interop.InitializeSecurity((uint)authenticationLevel);
                InitializeSecurityCalled = true;
            }
        }
        #endregion

        #region Internal Fields
        internal static bool InitializeSecurityCalled;
        #endregion

    }
}
