/*
 * [The "BSD licence"]
 * Copyright (c) 2005-2008 Terence Parr
 * All rights reserved.
 *
 * Conversion to C#:
 * Copyright (c) 2008 Sam Harwell, Pixel Mine, Inc.
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
using System.Runtime.Serialization;

namespace Antlr.Runtime {
    [Serializable]
    public class NoViableAltException : RecognitionException {
        public NoViableAltException() { }

        public NoViableAltException(string grammarDecisionDescription) => GrammarDecisionDescription = grammarDecisionDescription;

        public NoViableAltException(string message, string grammarDecisionDescription) : base(message) => GrammarDecisionDescription = grammarDecisionDescription;

        public NoViableAltException(string message, string grammarDecisionDescription, Exception innerException) : base(message, innerException) => GrammarDecisionDescription = grammarDecisionDescription;

        public NoViableAltException(string grammarDecisionDescription, int decisionNumber, int stateNumber, IIntStream input) : base(input) {
            GrammarDecisionDescription = grammarDecisionDescription;
            DecisionNumber = decisionNumber;
            StateNumber = stateNumber;
        }

        public NoViableAltException(string message, string grammarDecisionDescription, int decisionNumber, int stateNumber, IIntStream input) : base(message, input) {
            GrammarDecisionDescription = grammarDecisionDescription;
            DecisionNumber = decisionNumber;
            StateNumber = stateNumber;
        }

        public NoViableAltException(string message, string grammarDecisionDescription, int decisionNumber, int stateNumber, IIntStream input, int k) : base(message, input) {
            GrammarDecisionDescription = grammarDecisionDescription;
            DecisionNumber = decisionNumber;
            StateNumber = stateNumber;
        }

        public NoViableAltException(string grammarDecisionDescription, int decisionNumber, int stateNumber, IIntStream input, int k) : base(input) {
            GrammarDecisionDescription = grammarDecisionDescription;
            DecisionNumber = decisionNumber;
            StateNumber = stateNumber;
        }

        public NoViableAltException(string message, string grammarDecisionDescription, int decisionNumber, int stateNumber, IIntStream input, Exception innerException) : base(message, input, innerException) {
            GrammarDecisionDescription = grammarDecisionDescription;
            DecisionNumber = decisionNumber;
            StateNumber = stateNumber;
        }

        protected NoViableAltException(SerializationInfo info, StreamingContext context) : base(info, context) {
            if (info == null) {
                throw new ArgumentNullException("info");
            }
            GrammarDecisionDescription = info.GetString("GrammarDecisionDescription");
            DecisionNumber = info.GetInt32("DecisionNumber");
            StateNumber = info.GetInt32("StateNumber");
        }

        public int DecisionNumber { get; }

        public string GrammarDecisionDescription { get; }

        public int StateNumber { get; }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            if (info == null) {
                throw new ArgumentNullException("info");
            }
            base.GetObjectData(info, context);
            info.AddValue("GrammarDecisionDescription", GrammarDecisionDescription);
            info.AddValue("DecisionNumber", DecisionNumber);
            info.AddValue("StateNumber", StateNumber);
        }

        public override string ToString() {
            if (Input is ICharStream) {
                return "NoViableAltException('" + (char)UnexpectedType + "'@[" + GrammarDecisionDescription + "])";
            }
            return "NoViableAltException(" + UnexpectedType + "@[" + GrammarDecisionDescription + "])";
        }
    }
}