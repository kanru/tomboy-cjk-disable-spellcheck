/*
Copyright (c) 2006 Kanru Chen

Permission is hereby granted, free of charge, to any person obtaining a
copy of this software and associated documentation files (the "Software"),
to deal in the Software without restriction, including without limitation
the rights to use, copy, modify, merge, publish, distribute, sublicense,
and/or sell copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included
in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using Tomboy;

namespace Tomboy.CJKDisableSpell {
    public class CJKDisableSpellAddin : NoteAddin
    {
        NoteTag cjk_tag;

        public override void Initialize ()
        {
            if (Note.TagTable.Lookup ("lang:cjk") == null) {
                cjk_tag = new NoteTag ("lang:cjk");
                cjk_tag.CanUndo = true;
                cjk_tag.CanGrow = true; 
                cjk_tag.CanSpellCheck = false;
                Note.TagTable.Add (cjk_tag);
            } else {
                cjk_tag = (NoteTag)Note.TagTable.Lookup ("lang:cjk");
            }
        }
        public override void Shutdown ()
        {
            // Remove the tag only if we installed it.
            //if (cjk_tag != null)
            //	Note.TagTable.Remove (cjk_tag);
        }
        public override void OnNoteOpened ()
        {
            Buffer.InsertText += OnInsertText;
            Buffer.DeleteRange += OnDeleteRange;
        }
        void OnDeleteRange (object sender, Gtk.DeleteRangeArgs args)
        {
            ApplyCJKToBlock (args.Start, args.End);
        }
        void OnInsertText (object sender, Gtk.InsertTextArgs args)
        {                   
            Gtk.TextIter start = args.Pos;
            start.BackwardChars (args.Length);

            ApplyCJKToBlock (start, args.Pos);
        }
        void ApplyCJKToBlock (Gtk.TextIter start, Gtk.TextIter end)
        {
            NoteBuffer.GetBlockExtents (ref start,
                    ref end, 
                    512 /* XXX */,
                    cjk_tag);

            Buffer.RemoveTag (cjk_tag, start, end);

            MatchCJK m = new MatchCJK (start.GetText (end));
            foreach (MatchCJK.CJKGroup g in m) {
                Gtk.TextIter start_cpy = start;
                start_cpy.ForwardChars (g.Start);

                end = start;
                end.ForwardChars (g.End);

                Buffer.ApplyTag (cjk_tag, start_cpy, end);
            }
        }
    }
    public class MatchCJK : System.Collections.IEnumerable
    {
        string text;
        public MatchCJK (string text)
        {
            this.text = text;
        }
        public System.Collections.IEnumerator GetEnumerator()
        {
            int cur = 0;
            int start = 0;
            int end = 0;

            while (cur < text.Length)
            {
                while (cur < text.Length && IsCJK (text[cur]) != true) {
                    cur++;
                }
                start = cur;
                while (cur < text.Length && IsCJK (text[cur])) {
                    cur++;
                }
                end = cur;
                if (cur <= text.Length)
                    yield return new CJKGroup(start, end);
            }
        }
        public static bool IsCJK (char c) {
            double v = (double)c;
            if (0x2E80 < v && v < 0x2FA1D) /* Range of CJK in unicode */
                return true;
            else
                return false;
        }
        public class CJKGroup
        {
            public int Start;
            public int End;
            public CJKGroup (int s, int e)
            {
                Start = s;
                End = e;
            }
            public override string ToString ()
            {
                return String.Format("{0}, {1}", Start, End);
            }
        }
    }
}
