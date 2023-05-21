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
using System.Runtime.Serialization;
using Antlr.Runtime.Tree;

namespace Antlr.Runtime {
    /**
     * <summary>The root of the ANTLR exception hierarchy.</summary>
     * <remarks>
     *     To avoid English-only error messages and to generally make things
     *     as flexible as possible, these exceptions are not created with strings,
     *     but rather the information necessary to generate an error.  Then
     *     the various reporting methods in Parser and Lexer can be overridden
     *     to generate a localized error message.  For example, MismatchedToken
     *     exceptions are built with the expected token type.
     *     So, don't expect getMessage() to return anything.
     *     Note that as of Java 1.4, you can access the stack trace, which means
     *     that you can compute the complete trace of rules from the start symbol.
     *     This gives you considerable context information with which to generate
     *     useful error messages.
     *     ANTLR generates code that throws exceptions upon recognition error and
     *     also generates code to catch these exceptions in each rule.  If you
     *     want to quit upon first error, you can turn off the automatic error
     *     handling mechanism using rulecatch action, but you still need to
     *     override methods mismatch and recoverFromMismatchSet.
     *     In general, the recognition exceptions can track where in a grammar a
     *     problem occurred and/or what was the expected input.  While the parser
     *     knows its state (such as current input symbol and line info) that
     *     state can change before the exception is reported so current token index
     *     is computed and stored at exception time.  From this info, you can
     *     perhaps print an entire line of input not just a single token, for example.
     *     Better to just say the recognizer had a problem and then let the parser
     *     figure out a fancy report.
     * </remarks>
     */
    [Serializable]
    public class RecognitionException : Exception {
        /**
         * <summary>What input stream did the error occur in?</summary>
         */
        IIntStream _input;

        /**
         * <summary>What is index of token/char were we looking at when the error occurred?</summary>
         */
        int _index;

        /**
         * <summary>
         *     The current Token when an error occurred.  Since not all streams
         *     can retrieve the ith Token, we have to track the Token object.
         *     For parsers.  Even when it's a tree parser, token might be set.
         * </summary>
         */
        IToken _token;

        /**
         * <summary>
         *     If this is a tree parser exception, node is set to the node with
         *     the problem.
         * </summary>
         */
        object _node;

        /**
         * <summary>The current char when an error occurred. For lexers.</summary>
         */
        int _c;

        /**
         * <summary>
         *     Track the line (1-based) at which the error occurred in case this is
         *     generated from a lexer.  We need to track this since the
         *     unexpected char doesn't carry the line info.
         * </summary>
         */
        int _line;

        /// <summary>
        ///     The 0-based index into the line where the error occurred.
        /// </summary>
        int _charPositionInLine;

        /**
         * <summary>
         *     If you are parsing a tree node stream, you will encounter som
         *     imaginary nodes w/o line/col info.  We now search backwards looking
         *     for most recent token with line/col info, but notify getErrorHeader()
         *     that info is approximate.
         * </summary>
         */
        bool _approximateLineInfo;

        /**
         * <summary>Used for remote debugger deserialization</summary>
         */
        public RecognitionException() : this("A recognition error occurred.", null, null) { }

        public RecognitionException(IIntStream input) : this("A recognition error occurred.", input, null) { }

        public RecognitionException(string message) : this(message, null, null) { }

        public RecognitionException(string message, IIntStream input) : this(message, input, null) { }

        public RecognitionException(string message, Exception innerException) : this(message, null, innerException) { }

        public RecognitionException(string message, IIntStream input, Exception innerException) : base(message, innerException) {
            _input = input;
            if (input != null) {
                _index = input.Index;
                if (input is ITokenStream) {
                    _token = ((ITokenStream)input).LT(1);
                    _line = _token.Line;
                    _charPositionInLine = _token.CharPositionInLine;
                }
                ITreeNodeStream tns = input as ITreeNodeStream;
                if (tns != null) {
                    ExtractInformationFromTreeNodeStream(tns);
                }
                else {
                    ICharStream charStream = input as ICharStream;
                    if (charStream != null) {
                        _c = input.LA(1);
                        _line = ((ICharStream)input).Line;
                        _charPositionInLine = ((ICharStream)input).CharPositionInLine;
                    }
                    else {
                        _c = input.LA(1);
                    }
                }
            }
        }

        protected RecognitionException(SerializationInfo info, StreamingContext context)
            // : base(info, context)
        {
            if (info == null) {
                throw new ArgumentNullException("info");
            }
            _index = info.GetInt32("Index");
            _c = info.GetInt32("C");
            _line = info.GetInt32("Line");
            _charPositionInLine = info.GetInt32("CharPositionInLine");
            _approximateLineInfo = info.GetBoolean("ApproximateLineInfo");
        }

        /**
         * <summary>Return the token type or char of the unexpected input element</summary>
         */
        public virtual int UnexpectedType {
            get {
                if (_input is ITokenStream) {
                    return _token.Type;
                }
                ITreeNodeStream treeNodeStream = _input as ITreeNodeStream;
                if (treeNodeStream != null) {
                    ITreeAdaptor adaptor = treeNodeStream.TreeAdaptor;
                    return adaptor.GetType(_node);
                }
                return _c;
            }
        }

        public bool ApproximateLineInfo {
            get => _approximateLineInfo;
            protected set => _approximateLineInfo = value;
        }

        public IIntStream Input {
            get => _input;
            protected set => _input = value;
        }

        public IToken Token {
            get => _token;
            set => _token = value;
        }

        public object Node {
            get => _node;
            protected set => _node = value;
        }

        public int Character {
            get => _c;
            protected set => _c = value;
        }

        public int Index {
            get => _index;
            protected set => _index = value;
        }

        public int Line {
            get => _line;
            set => _line = value;
        }

        public int CharPositionInLine {
            get => _charPositionInLine;
            set => _charPositionInLine = value;
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context) {
            if (info == null) {
                throw new ArgumentNullException("info");
            }

            //base.GetObjectData(info, context);
            info.AddValue("Index", _index);
            info.AddValue("C", _c);
            info.AddValue("Line", _line);
            info.AddValue("CharPositionInLine", _charPositionInLine);
            info.AddValue("ApproximateLineInfo", _approximateLineInfo);
        }

        protected virtual void ExtractInformationFromTreeNodeStream(ITreeNodeStream input) {
            _node = input.LT(1);
            ITokenStreamInformation streamInformation = input as ITokenStreamInformation;
            if (streamInformation != null) {
                IToken lastToken = streamInformation.LastToken;
                IToken lastRealToken = streamInformation.LastRealToken;
                if (lastRealToken != null) {
                    _token = lastRealToken;
                    _line = lastRealToken.Line;
                    _charPositionInLine = lastRealToken.CharPositionInLine;
                    _approximateLineInfo = lastRealToken.Equals(lastToken);
                }
            }
            else {
                ITreeAdaptor adaptor = input.TreeAdaptor;
                IToken payload = adaptor.GetToken(_node);
                if (payload != null) {
                    _token = payload;
                    if (payload.Line <= 0) {
                        // imaginary node; no line/pos info; scan backwards
                        int i = -1;
                        object priorNode = input.LT(i);
                        while (priorNode != null) {
                            IToken priorPayload = adaptor.GetToken(priorNode);
                            if (priorPayload != null
                                && priorPayload.Line > 0) {
                                // we found the most recent real line / pos info
                                _line = priorPayload.Line;
                                _charPositionInLine = priorPayload.CharPositionInLine;
                                _approximateLineInfo = true;
                                break;
                            }
                            --i;
                            try {
                                priorNode = input.LT(i);
                            }
                            catch (ArgumentException) {
                                priorNode = null;
                            }
                        }
                    }
                    else {
                        // node created from real token
                        _line = payload.Line;
                        _charPositionInLine = payload.CharPositionInLine;
                    }
                }
                else if (_node is ITree) {
                    _line = ((ITree)_node).Line;
                    _charPositionInLine = ((ITree)_node).CharPositionInLine;
                    if (_node is CommonTree) {
                        _token = ((CommonTree)_node).Token;
                    }
                }
                else {
                    int type = adaptor.GetType(_node);
                    string text = adaptor.GetText(_node);
                    _token = new CommonToken(type, text);
                }
            }
        }
    }
}