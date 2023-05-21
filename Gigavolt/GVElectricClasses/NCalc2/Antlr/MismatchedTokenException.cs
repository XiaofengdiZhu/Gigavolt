/*
 * [The "BSD licence"]
 * Copyright (c) 2005-2008 Terence Parr
 * All rights reserved.
 *
 * Conversion to C#:
 * Copyright (c) 2008-2009 Sam Harwell, Pixel Mine, Inc.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 *    notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 *    notice, this list of conditions and the following disclaimer in the
 *    documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 *    derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;

namespace Antlr.Runtime {
    /**
     * <summary>A mismatched char or Token or tree node</summary>
     */
    [Serializable]
    public class MismatchedTokenException : RecognitionException {
        public MismatchedTokenException() { }

        public MismatchedTokenException(string message) : base(message) { }

        public MismatchedTokenException(string message, Exception innerException) : base(message, innerException) { }

        public MismatchedTokenException(int expecting, IIntStream input) : this(expecting, input, null) { }

        public MismatchedTokenException(int expecting, IIntStream input, IList<string> tokenNames) : base(input) {
            Expecting = expecting;
            if (tokenNames != null) {
                TokenNames = tokenNames.ToList().AsReadOnly();
            }
        }

        public MismatchedTokenException(string message, int expecting, IIntStream input, IList<string> tokenNames) : base(message, input) {
            Expecting = expecting;
            if (tokenNames != null) {
                TokenNames = tokenNames.ToList().AsReadOnly();
            }
        }

        public MismatchedTokenException(string message, int expecting, IIntStream input, IList<string> tokenNames, Exception innerException) : base(message, input, innerException) {
            Expecting = expecting;
            if (tokenNames != null) {
                TokenNames = tokenNames.ToList().AsReadOnly();
            }
        }

        protected MismatchedTokenException(SerializationInfo info, StreamingContext context) : base(info, context) {
            if (info == null) {
                throw new ArgumentNullException("info");
            }
            Expecting = info.GetInt32("Expecting");
            TokenNames = new ReadOnlyCollection<string>((string[])info.GetValue("TokenNames", typeof(string[])));
        }

        public int Expecting { get; } = TokenTypes.Invalid;

        public ReadOnlyCollection<string> TokenNames { get; }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            if (info == null) {
                throw new ArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("Expecting", Expecting);
            info.AddValue("TokenNames", TokenNames != null ? TokenNames.ToArray() : default);
        }

        public override string ToString() {
            int unexpectedType = UnexpectedType;
            string unexpected = TokenNames != null && unexpectedType >= 0 && unexpectedType < TokenNames.Count ? TokenNames[unexpectedType] : unexpectedType.ToString();
            string expected = TokenNames != null && Expecting >= 0 && Expecting < TokenNames.Count ? TokenNames[Expecting] : Expecting.ToString();
            return "MismatchedTokenException(" + unexpected + "!=" + expected + ")";
        }
    }
}