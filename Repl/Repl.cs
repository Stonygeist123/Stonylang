using Stonylang.Evaluator;
using Stonylang.Utility;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Collections.Specialized;
using Stonylang.Lexer;
using Stonylang.SyntaxFacts;
using Stonylang.Parser;

namespace Stonylang
{
    internal abstract class Repl
    {
        private readonly List<string> _submissionHistory = new();
        private int _submissionHistoryIndex;
        private bool _done;
        public void Run()
        {
            while (true)
            {
                string text = EditSubmission();
                if (string.IsNullOrWhiteSpace(text))
                    return;

                if (!text.Contains(Environment.NewLine) && text.StartsWith("#"))
                {
                    EvaluateMetaCommand(text);
                    continue;
                }

                EvaluateSubmission(text);
                _submissionHistory.Add(text);
                _submissionHistoryIndex = 0;
            }
        }

        private sealed class SubmissionView
        {
            private readonly Action<string> _lineRenderer;
            private readonly ObservableCollection<string> _submissionDocument;
            private readonly int _curserTop;
            private int _renderedLineCount;
            private int _currentLine;
            private int _currentCharacter;

            public SubmissionView(Action<string> lineRenderer, ObservableCollection<string> submissionDocument)
            {
                _lineRenderer = lineRenderer;
                _submissionDocument = submissionDocument;
                _submissionDocument.CollectionChanged += SubmissionDocumentChanged;
                _curserTop = Console.CursorTop;
                Render();
            }

            private void SubmissionDocumentChanged(object sender, NotifyCollectionChangedEventArgs e) => Render();
            private void Render()
            {
                Console.CursorVisible = false;
                int lineCount = 0;

                foreach (string line in _submissionDocument)
                {
                    Console.SetCursorPosition(0, _curserTop + lineCount);
                    Console.ForegroundColor = ConsoleColor.Green;
                    if (lineCount == 0)
                        Console.Write("» ");
                    else
                        Console.Write("· ");

                    Console.ResetColor();
                    _lineRenderer(line);
                    Console.Write(new string(' ', Console.WindowWidth - line.Length - 2));
                    ++lineCount;
                }

                int numberOfBlankLines = _renderedLineCount - lineCount;
                if (numberOfBlankLines > 0)
                {
                    string blankLine = new(' ', Console.WindowWidth);
                    for (int i = 0; i < numberOfBlankLines; ++i)
                    {
                        Console.SetCursorPosition(0, _curserTop + lineCount + i);
                        Console.WriteLine(blankLine);
                    }
                }

                _renderedLineCount = lineCount;
                Console.CursorVisible = true;
                UpdateCursorPosition();
            }

            private void UpdateCursorPosition()
            {
                Console.CursorTop = _curserTop + _currentLine;
                Console.CursorLeft = 2 + _currentCharacter;
            }

            public int CurrentLine
            {
                get => _currentLine;
                set
                {
                    if (_currentLine != value)
                    {
                        _currentLine = value;
                        _currentCharacter = Math.Min(_submissionDocument[_currentLine].Length, _currentCharacter);
                        UpdateCursorPosition();
                    }
                }
            }
            public int CurrentCharacter
            {
                get => _currentCharacter;
                set
                {
                    if (_currentCharacter != value)
                    {
                        _currentCharacter = value;
                        UpdateCursorPosition();
                    }
                }
            }
        }

        private string EditSubmission()
        {
            _done = false;
            ObservableCollection<string> document = new() { "" };
            SubmissionView view = new(RenderLine, document);

            while (!_done)
            {
                ConsoleKeyInfo key = Console.ReadKey();
                HandleKey(key, document, view);
            }

            view.CurrentLine = document.Count - 1;
            view.CurrentCharacter = document[view.CurrentLine].Length;
            Console.WriteLine();

            return string.Join(Environment.NewLine, document);
        }

        private void HandleKey(ConsoleKeyInfo key, ObservableCollection<string> document, SubmissionView view)
        {
            if (key.Modifiers == default)
                switch (key.Key)
                {
                    case ConsoleKey.Escape:
                        HandleEscape(document, view);
                        break;
                    case ConsoleKey.Enter:
                        HandleEnter(document, view);
                        break;
                    case ConsoleKey.LeftArrow:
                        HandleLeftArrow(view);
                        break;
                    case ConsoleKey.RightArrow:
                        HandleRightArrow(document, view);
                        break;
                    case ConsoleKey.UpArrow:
                        HandleUpArrow(view);
                        break;
                    case ConsoleKey.DownArrow:
                        HandleDownArrow(document, view);
                        break;
                    case ConsoleKey.Backspace:
                        HandleBackSpace(document, view);
                        break;
                    case ConsoleKey.Delete:
                        HandleDelete(document, view);
                        break;
                    case ConsoleKey.Home:
                        HandleHome(view);
                        break;
                    case ConsoleKey.End:
                        HandleEnd(document, view);
                        break;
                    case ConsoleKey.Tab:
                        HandleTab(document, view);
                        break;
                    case ConsoleKey.PageUp:
                        HandlePageUp(document, view);
                        break;
                    case ConsoleKey.PageDown:
                        HandlePageDown(document, view);
                        break;
                }
            else if (key.Modifiers == ConsoleModifiers.Control)
                switch (key.Key)
                {
                    case ConsoleKey.Enter:
                        HandleControlEnter(document, view);
                        break;
                }

            if (key.KeyChar >= ' ')
                HandleTyping(document, view, key.KeyChar.ToString());
        }

        private static void HandleEscape(ObservableCollection<string> document, SubmissionView view)
        {
            document[view.CurrentLine] = string.Empty;
            view.CurrentCharacter = 0;
        }

        private static void HandleControlEnter(ObservableCollection<string> document, SubmissionView view) => InsertLine(document, view);
        private void HandleEnter(ObservableCollection<string> document, SubmissionView view)
        {
            string submissionText = string.Join(Environment.NewLine, document);
            if (submissionText.StartsWith('#') || IsCompleteSubmission(submissionText))
            {
                _done = true;
                return;
            }

            InsertLine(document, view);
        }

        private static void InsertLine(ObservableCollection<string> document, SubmissionView view)
        {
            string remainder = document[view.CurrentLine][view.CurrentCharacter..];
            document[view.CurrentLine] = document[view.CurrentLine].Substring(0, view.CurrentCharacter);

            int lineIndex = view.CurrentLine + 1;
            document.Insert(lineIndex, remainder);
            view.CurrentCharacter = 0;
            view.CurrentLine = lineIndex;
        }

        private static void HandleLeftArrow(SubmissionView view)
        {
            if (view.CurrentCharacter > 0)
                --view.CurrentCharacter;
        }

        private static void HandleRightArrow(ObservableCollection<string> document, SubmissionView view)
        {
            string line = document[view.CurrentLine];
            if (view.CurrentCharacter <= line.Length - 1)
                ++view.CurrentCharacter;
        }

        private static void HandleUpArrow(SubmissionView view)
        {
            if (view.CurrentLine > 0)
                --view.CurrentLine;
        }

        private static void HandleDownArrow(ObservableCollection<string> document, SubmissionView view)
        {
            if (view.CurrentLine < document.Count - 1)
                ++view.CurrentLine;
        }

        private static void HandleBackSpace(ObservableCollection<string> document, SubmissionView view)
        {
            int start = view.CurrentCharacter;
            if (start == 0)
            {
                if (view.CurrentLine == 0)
                    return;

                string currentLine = document[view.CurrentLine];
                string previousLine = document[view.CurrentLine - 1];
                document.RemoveAt(view.CurrentLine);
                --view.CurrentLine;
                document[view.CurrentLine] = previousLine + currentLine;
                view.CurrentCharacter = previousLine.Length;
            }
            else
            {
                int lineIndex = view.CurrentLine;
                string line = document[lineIndex];
                string before = line.Substring(0, start - 1);
                string after = line[start..];
                document[lineIndex] = before + after;
                view.CurrentCharacter--;
            }
        }

        private static void HandleDelete(ObservableCollection<string> document, SubmissionView view)
        {
            int lineIndex = view.CurrentLine;
            string line = document[lineIndex];
            int start = view.CurrentCharacter;
            if (start >= line.Length)
            {
                if (view.CurrentLine == document.Count - 1)
                    return;

                string newLine = document[view.CurrentLine + 1];
                document[view.CurrentLine] += newLine;
                document.RemoveAt(view.CurrentLine + 1);
                return;
            }

            string before = line.Substring(0, start);
            string after = line[(start + 1)..];
            document[lineIndex] = before + after;
        }

        private static void HandleHome(SubmissionView view) => view.CurrentCharacter = 0;
        private static void HandleEnd(ObservableCollection<string> document, SubmissionView view) => view.CurrentCharacter = document[view.CurrentLine].Length;
        private static void HandleTab(ObservableCollection<string> document, SubmissionView view)
        {
            const int TabWidth = 4;
            int start = view.CurrentCharacter;
            int remainingSpaces = TabWidth - start % TabWidth;
            string line = document[view.CurrentLine];
            document[view.CurrentLine] = line.Insert(start, new(' ', remainingSpaces));
            view.CurrentCharacter += remainingSpaces;
        }

        private void HandlePageUp(ObservableCollection<string> document, SubmissionView view)
        {
            --_submissionHistoryIndex;
            if (_submissionHistoryIndex < 0)
                _submissionHistoryIndex = _submissionHistory.Count - 1;
            UpdateDocumentFromHistory(document, view);
        }

        private void HandlePageDown(ObservableCollection<string> document, SubmissionView view)
        {
            ++_submissionHistoryIndex;
            if (_submissionHistoryIndex > _submissionHistory.Count - 1)
                _submissionHistoryIndex = 0;
            UpdateDocumentFromHistory(document, view);
        }

        private void UpdateDocumentFromHistory(ObservableCollection<string> document, SubmissionView view)
        {
            if (_submissionHistory.Count == 0)
                return;

            document.Clear();
            string historyItem = _submissionHistory[_submissionHistoryIndex];
            foreach (string line in historyItem.Split(Environment.NewLine))
                document.Add(line);

            view.CurrentLine = document.Count - 1;
            view.CurrentCharacter = document[view.CurrentLine].Length;
        }

        private static void HandleTyping(ObservableCollection<string> document, SubmissionView view, string text)
        {
            int lineIndex = view.CurrentLine;
            int start = view.CurrentCharacter;
            document[lineIndex] = document[lineIndex].Insert(start, text);
            ++view.CurrentCharacter;
        }

        protected virtual void RenderLine(string line)
        {
            Console.Write(line);
        }

        protected void ClearHistory() => _submissionHistory.Clear();
        protected abstract void EvaluateMetaCommand(string input);
        protected abstract bool IsCompleteSubmission(string text);
        protected abstract void EvaluateSubmission(string input);
    }

    internal sealed class StonyRepl : Repl
    {
        private Compilation _previous;
        private bool _showTree = false, _showProgram = false;
        private readonly Dictionary<string, VariableSymbol> symbolTable = new();

        protected override void RenderLine(string line)
        {
            IEnumerable<Token> tokens = SyntaxTree.SyntaxTree.ParseTokens(line);
            foreach (Token token in tokens)
            {
                bool isKeyword = token.Kind.ToString()?.EndsWith("Keyword") ?? false;
                bool isNumber = token.Kind == SyntaxKind.Number;
                bool isIdentifier = token.Kind == SyntaxKind.Identifier;
                if (isKeyword)
                    Console.ForegroundColor = ConsoleColor.Blue;
                else if (isIdentifier)
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                else if (!isNumber)
                    Console.ForegroundColor = ConsoleColor.DarkGray;

                Console.Write(token.Lexeme);
                Console.ResetColor();
            }
        }

        protected override bool IsCompleteSubmission(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;

            SyntaxTree.SyntaxTree syntaxTree = SyntaxTree.SyntaxTree.Parse(text);
            return !GetLastToken(syntaxTree.Root.Statement).IsMissing;
        }

        private Token GetLastToken(Node node) => node is Token t ? t : GetLastToken(node.GetChildren().Last());
        protected override void EvaluateMetaCommand(string input)
        {
            switch (input.ToLower())
            {
                case "#showtree":
                    _showTree ^= true;
                    Console.WriteLine(_showTree ? "Showing parse trees." : "Not showing parse trees.");
                    break;
                case "#showprogram":
                    _showProgram ^= true;
                    Console.WriteLine(_showProgram ? "Showing program." : "Not showing program.");
                    break;
                case "#cls":
                    Console.Clear();
                    break;
                case "#reset":
                    _previous = null;
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Invalid command \"{input}\"");
                    Console.ResetColor();
                    break;
            }
        }

        protected override void EvaluateSubmission(string input)
        {
            input = input.TrimEnd();
            SourceText source = SourceText.From(input);
            SyntaxTree.SyntaxTree syntaxTree = SyntaxTree.SyntaxTree.Parse(input);

            Compilation compilation = _previous == null
                ? new(syntaxTree, source) : _previous.ContinueWith(syntaxTree, source);
            EvaluationResult result = compilation.Evaluate(symbolTable);
            DiagnosticBag diagnostics = result.Diagnostics;

            if (diagnostics.Any())
                foreach (Diagnostic diagnostic in diagnostics)
                    diagnostic.Print();
            else
            {
                _previous = compilation;
                if (_showTree)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    syntaxTree.Root.WriteTo(Console.Out);
                    Console.ResetColor();
                }
                if (_showProgram)
                    compilation.EmitTree(Console.Out);

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(result.Value);
                Console.ResetColor();
            }
        }
    }
}
