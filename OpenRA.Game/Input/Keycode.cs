#region Copyright & License Information
/*
 * Copyright 2007-2019 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation, either version 3 of
 * the License, or (at your option) any later version. For more
 * information, see COPYING.
 */
#endregion

using System.Collections.Generic;

namespace OpenRA
{
	// List of keycodes, duplicated from SDL 2.0.1
	public enum Keycode
	{
		UNKNOWN = 0,
		RETURN = '\r',
		ESCAPE = 27,
		BACKSPACE = '\b',
		TAB = '\t',
		SPACE = ' ',
		EXCLAIM = '!',
		QUOTEDBL = '"',
		HASH = '#',
		PERCENT = '%',
		DOLLAR = '$',
		AMPERSAND = '&',
		QUOTE = '\'',
		LEFTPAREN = '(',
		RIGHTPAREN = ')',
		ASTERISK = '*',
		PLUS = '+',
		COMMA = ',',
		MINUS = '-',
		PERIOD = '.',
		SLASH = '/',
		NUMBER_0 = '0',
		NUMBER_1 = '1',
		NUMBER_2 = '2',
		NUMBER_3 = '3',
		NUMBER_4 = '4',
		NUMBER_5 = '5',
		NUMBER_6 = '6',
		NUMBER_7 = '7',
		NUMBER_8 = '8',
		NUMBER_9 = '9',
		COLON = ':',
		SEMICOLON = ';',
		LESS = '<',
		EQUALS = '=',
		GREATER = '>',
		QUESTION = '?',
		AT = '@',
		LEFTBRACKET = '[',
		BACKSLASH = '\\',
		RIGHTBRACKET = ']',
		CARET = '^',
		UNDERSCORE = '_',
		BACKQUOTE = '`',
		A = 'a',
		B = 'b',
		C = 'c',
		D = 'd',
		E = 'e',
		F = 'f',
		G = 'g',
		H = 'h',
		I = 'i',
		J = 'j',
		K = 'k',
		L = 'l',
		M = 'm',
		N = 'n',
		O = 'o',
		P = 'p',
		Q = 'q',
		R = 'r',
		S = 's',
		T = 't',
		U = 'u',
		V = 'v',
		W = 'w',
		X = 'x',
		Y = 'y',
		Z = 'z',
		CAPSLOCK = 57 | (1 << 30),
		F1 = 58 | (1 << 30),
		F2 = 59 | (1 << 30),
		F3 = 60 | (1 << 30),
		F4 = 61 | (1 << 30),
		F5 = 62 | (1 << 30),
		F6 = 63 | (1 << 30),
		F7 = 64 | (1 << 30),
		F8 = 65 | (1 << 30),
		F9 = 66 | (1 << 30),
		F10 = 67 | (1 << 30),
		F11 = 68 | (1 << 30),
		F12 = 69 | (1 << 30),
		PRINTSCREEN = 70 | (1 << 30),
		SCROLLLOCK = 71 | (1 << 30),
		PAUSE = 72 | (1 << 30),
		INSERT = 73 | (1 << 30),
		HOME = 74 | (1 << 30),
		PAGEUP = 75 | (1 << 30),
		DELETE = 127,
		END = 77 | (1 << 30),
		PAGEDOWN = 78 | (1 << 30),
		RIGHT = 79 | (1 << 30),
		LEFT = 80 | (1 << 30),
		DOWN = 81 | (1 << 30),
		UP = 82 | (1 << 30),
		NUMLOCKCLEAR = 83 | (1 << 30),
		KP_DIVIDE = 84 | (1 << 30),
		KP_MULTIPLY = 85 | (1 << 30),
		KP_MINUS = 86 | (1 << 30),
		KP_PLUS = 87 | (1 << 30),
		KP_ENTER = 88 | (1 << 30),
		KP_1 = 89 | (1 << 30),
		KP_2 = 90 | (1 << 30),
		KP_3 = 91 | (1 << 30),
		KP_4 = 92 | (1 << 30),
		KP_5 = 93 | (1 << 30),
		KP_6 = 94 | (1 << 30),
		KP_7 = 95 | (1 << 30),
		KP_8 = 96 | (1 << 30),
		KP_9 = 97 | (1 << 30),
		KP_0 = 98 | (1 << 30),
		KP_PERIOD = 99 | (1 << 30),
		APPLICATION = 101 | (1 << 30),
		POWER = 102 | (1 << 30),
		KP_EQUALS = 103 | (1 << 30),
		F13 = 104 | (1 << 30),
		F14 = 105 | (1 << 30),
		F15 = 106 | (1 << 30),
		F16 = 107 | (1 << 30),
		F17 = 108 | (1 << 30),
		F18 = 109 | (1 << 30),
		F19 = 110 | (1 << 30),
		F20 = 111 | (1 << 30),
		F21 = 112 | (1 << 30),
		F22 = 113 | (1 << 30),
		F23 = 114 | (1 << 30),
		F24 = 115 | (1 << 30),
		EXECUTE = 116 | (1 << 30),
		HELP = 117 | (1 << 30),
		MENU = 118 | (1 << 30),
		SELECT = 119 | (1 << 30),
		STOP = 120 | (1 << 30),
		AGAIN = 121 | (1 << 30),
		UNDO = 122 | (1 << 30),
		CUT = 123 | (1 << 30),
		COPY = 124 | (1 << 30),
		PASTE = 125 | (1 << 30),
		FIND = 126 | (1 << 30),
		MUTE = 127 | (1 << 30),
		VOLUMEUP = 128 | (1 << 30),
		VOLUMEDOWN = 129 | (1 << 30),
		KP_COMMA = 133 | (1 << 30),
		KP_EQUALSAS400 = 134 | (1 << 30),
		ALTERASE = 153 | (1 << 30),
		SYSREQ = 154 | (1 << 30),
		CANCEL = 155 | (1 << 30),
		CLEAR = 156 | (1 << 30),
		PRIOR = 157 | (1 << 30),
		RETURN2 = 158 | (1 << 30),
		SEPARATOR = 159 | (1 << 30),
		OUT = 160 | (1 << 30),
		OPER = 161 | (1 << 30),
		CLEARAGAIN = 162 | (1 << 30),
		CRSEL = 163 | (1 << 30),
		EXSEL = 164 | (1 << 30),
		KP_00 = 176 | (1 << 30),
		KP_000 = 177 | (1 << 30),
		THOUSANDSSEPARATOR = 178 | (1 << 30),
		DECIMALSEPARATOR = 179 | (1 << 30),
		CURRENCYUNIT = 180 | (1 << 30),
		CURRENCYSUBUNIT = 181 | (1 << 30),
		KP_LEFTPAREN = 182 | (1 << 30),
		KP_RIGHTPAREN = 183 | (1 << 30),
		KP_LEFTBRACE = 184 | (1 << 30),
		KP_RIGHTBRACE = 185 | (1 << 30),
		KP_TAB = 186 | (1 << 30),
		KP_BACKSPACE = 187 | (1 << 30),
		KP_A = 188 | (1 << 30),
		KP_B = 189 | (1 << 30),
		KP_C = 190 | (1 << 30),
		KP_D = 191 | (1 << 30),
		KP_E = 192 | (1 << 30),
		KP_F = 193 | (1 << 30),
		KP_XOR = 194 | (1 << 30),
		KP_POWER = 195 | (1 << 30),
		KP_PERCENT = 196 | (1 << 30),
		KP_LESS = 197 | (1 << 30),
		KP_GREATER = 198 | (1 << 30),
		KP_AMPERSAND = 199 | (1 << 30),
		KP_DBLAMPERSAND = 200 | (1 << 30),
		KP_VERTICALBAR = 201 | (1 << 30),
		KP_DBLVERTICALBAR = 202 | (1 << 30),
		KP_COLON = 203 | (1 << 30),
		KP_HASH = 204 | (1 << 30),
		KP_SPACE = 205 | (1 << 30),
		KP_AT = 206 | (1 << 30),
		KP_EXCLAM = 207 | (1 << 30),
		KP_MEMSTORE = 208 | (1 << 30),
		KP_MEMRECALL = 209 | (1 << 30),
		KP_MEMCLEAR = 210 | (1 << 30),
		KP_MEMADD = 211 | (1 << 30),
		KP_MEMSUBTRACT = 212 | (1 << 30),
		KP_MEMMULTIPLY = 213 | (1 << 30),
		KP_MEMDIVIDE = 214 | (1 << 30),
		KP_PLUSMINUS = 215 | (1 << 30),
		KP_CLEAR = 216 | (1 << 30),
		KP_CLEARENTRY = 217 | (1 << 30),
		KP_BINARY = 218 | (1 << 30),
		KP_OCTAL = 219 | (1 << 30),
		KP_DECIMAL = 220 | (1 << 30),
		KP_HEXADECIMAL = 221 | (1 << 30),
		LCTRL = 224 | (1 << 30),
		LSHIFT = 225 | (1 << 30),
		LALT = 226 | (1 << 30),
		LGUI = 227 | (1 << 30),
		RCTRL = 228 | (1 << 30),
		RSHIFT = 229 | (1 << 30),
		RALT = 230 | (1 << 30),
		RGUI = 231 | (1 << 30),
		MODE = 257 | (1 << 30),
		AUDIONEXT = 258 | (1 << 30),
		AUDIOPREV = 259 | (1 << 30),
		AUDIOSTOP = 260 | (1 << 30),
		AUDIOPLAY = 261 | (1 << 30),
		AUDIOMUTE = 262 | (1 << 30),
		MEDIASELECT = 263 | (1 << 30),
		WWW = 264 | (1 << 30),
		MAIL = 265 | (1 << 30),
		CALCULATOR = 266 | (1 << 30),
		COMPUTER = 267 | (1 << 30),
		AC_SEARCH = 268 | (1 << 30),
		AC_HOME = 269 | (1 << 30),
		AC_BACK = 270 | (1 << 30),
		AC_FORWARD = 271 | (1 << 30),
		AC_STOP = 272 | (1 << 30),
		AC_REFRESH = 273 | (1 << 30),
		AC_BOOKMARKS = 274 | (1 << 30),
		BRIGHTNESSDOWN = 275 | (1 << 30),
		BRIGHTNESSUP = 276 | (1 << 30),
		DISPLAYSWITCH = 277 | (1 << 30),
		KBDILLUMTOGGLE = 278 | (1 << 30),
		KBDILLUMDOWN = 279 | (1 << 30),
		KBDILLUMUP = 280 | (1 << 30),
		EJECT = 281 | (1 << 30),
		SLEEP = 282 | (1 << 30),
	}

	public static class KeycodeExts
	{
		static readonly Dictionary<Keycode, string> KeyNames = new Dictionary<Keycode, string>
		{
			{ Keycode.UNKNOWN, "Undefined" },
			{ Keycode.RETURN, "Return" },
			{ Keycode.ESCAPE, "Escape" },
			{ Keycode.BACKSPACE, "Backspace" },
			{ Keycode.TAB, "Tab" },
			{ Keycode.SPACE, "Space" },
			{ Keycode.EXCLAIM, "!" },
			{ Keycode.QUOTEDBL, "\"" },
			{ Keycode.HASH, "#" },
			{ Keycode.PERCENT, "%" },
			{ Keycode.DOLLAR, "$" },
			{ Keycode.AMPERSAND, "&" },
			{ Keycode.QUOTE, "'" },
			{ Keycode.LEFTPAREN, "(" },
			{ Keycode.RIGHTPAREN, ")" },
			{ Keycode.ASTERISK, "*" },
			{ Keycode.PLUS, "+" },
			{ Keycode.COMMA, "," },
			{ Keycode.MINUS, "-" },
			{ Keycode.PERIOD, "." },
			{ Keycode.SLASH, "/" },
			{ Keycode.NUMBER_0, "0" },
			{ Keycode.NUMBER_1, "1" },
			{ Keycode.NUMBER_2, "2" },
			{ Keycode.NUMBER_3, "3" },
			{ Keycode.NUMBER_4, "4" },
			{ Keycode.NUMBER_5, "5" },
			{ Keycode.NUMBER_6, "6" },
			{ Keycode.NUMBER_7, "7" },
			{ Keycode.NUMBER_8, "8" },
			{ Keycode.NUMBER_9, "9" },
			{ Keycode.COLON, ":" },
			{ Keycode.SEMICOLON, ";" },
			{ Keycode.LESS, "<" },
			{ Keycode.EQUALS, "=" },
			{ Keycode.GREATER, ">" },
			{ Keycode.QUESTION, "?" },
			{ Keycode.AT, "@" },
			{ Keycode.LEFTBRACKET, "[" },
			{ Keycode.BACKSLASH, "\\" },
			{ Keycode.RIGHTBRACKET, "]" },
			{ Keycode.CARET, "^" },
			{ Keycode.UNDERSCORE, "_" },
			{ Keycode.BACKQUOTE, "`" },
			{ Keycode.A, "A" },
			{ Keycode.B, "B" },
			{ Keycode.C, "C" },
			{ Keycode.D, "D" },
			{ Keycode.E, "E" },
			{ Keycode.F, "F" },
			{ Keycode.G, "G" },
			{ Keycode.H, "H" },
			{ Keycode.I, "I" },
			{ Keycode.J, "J" },
			{ Keycode.K, "K" },
			{ Keycode.L, "L" },
			{ Keycode.M, "M" },
			{ Keycode.N, "N" },
			{ Keycode.O, "O" },
			{ Keycode.P, "P" },
			{ Keycode.Q, "Q" },
			{ Keycode.R, "R" },
			{ Keycode.S, "S" },
			{ Keycode.T, "T" },
			{ Keycode.U, "U" },
			{ Keycode.V, "V" },
			{ Keycode.W, "W" },
			{ Keycode.X, "X" },
			{ Keycode.Y, "Y" },
			{ Keycode.Z, "Z" },
			{ Keycode.CAPSLOCK, "CapsLock" },
			{ Keycode.F1, "F1" },
			{ Keycode.F2, "F2" },
			{ Keycode.F3, "F3" },
			{ Keycode.F4, "F4" },
			{ Keycode.F5, "F5" },
			{ Keycode.F6, "F6" },
			{ Keycode.F7, "F7" },
			{ Keycode.F8, "F8" },
			{ Keycode.F9, "F9" },
			{ Keycode.F10, "F10" },
			{ Keycode.F11, "F11" },
			{ Keycode.F12, "F12" },
			{ Keycode.PRINTSCREEN, "PrintScreen" },
			{ Keycode.SCROLLLOCK, "ScrollLock" },
			{ Keycode.PAUSE, "Pause" },
			{ Keycode.INSERT, "Insert" },
			{ Keycode.HOME, "Home" },
			{ Keycode.PAGEUP, "PageUp" },
			{ Keycode.DELETE, "Delete" },
			{ Keycode.END, "End" },
			{ Keycode.PAGEDOWN, "PageDown" },
			{ Keycode.RIGHT, "Right" },
			{ Keycode.LEFT, "Left" },
			{ Keycode.DOWN, "Down" },
			{ Keycode.UP, "Up" },
			{ Keycode.NUMLOCKCLEAR, "Numlock" },
			{ Keycode.KP_DIVIDE, "Keypad /" },
			{ Keycode.KP_MULTIPLY, "Keypad *" },
			{ Keycode.KP_MINUS, "Keypad -" },
			{ Keycode.KP_PLUS, "Keypad +" },
			{ Keycode.KP_ENTER, "Keypad Enter" },
			{ Keycode.KP_1, "Keypad 1" },
			{ Keycode.KP_2, "Keypad 2" },
			{ Keycode.KP_3, "Keypad 3" },
			{ Keycode.KP_4, "Keypad 4" },
			{ Keycode.KP_5, "Keypad 5" },
			{ Keycode.KP_6, "Keypad 6" },
			{ Keycode.KP_7, "Keypad 7" },
			{ Keycode.KP_8, "Keypad 8" },
			{ Keycode.KP_9, "Keypad 9" },
			{ Keycode.KP_0, "Keypad 0" },
			{ Keycode.KP_PERIOD, "Keypad ." },
			{ Keycode.APPLICATION, "Application" },
			{ Keycode.POWER, "Power" },
			{ Keycode.KP_EQUALS, "Keypad =" },
			{ Keycode.F13, "F13" },
			{ Keycode.F14, "F14" },
			{ Keycode.F15, "F15" },
			{ Keycode.F16, "F16" },
			{ Keycode.F17, "F17" },
			{ Keycode.F18, "F18" },
			{ Keycode.F19, "F19" },
			{ Keycode.F20, "F20" },
			{ Keycode.F21, "F21" },
			{ Keycode.F22, "F22" },
			{ Keycode.F23, "F23" },
			{ Keycode.F24, "F24" },
			{ Keycode.EXECUTE, "Execute" },
			{ Keycode.HELP, "Help" },
			{ Keycode.MENU, "Menu" },
			{ Keycode.SELECT, "Select" },
			{ Keycode.STOP, "Stop" },
			{ Keycode.AGAIN, "Again" },
			{ Keycode.UNDO, "Undo" },
			{ Keycode.CUT, "Cut" },
			{ Keycode.COPY, "Copy" },
			{ Keycode.PASTE, "Paste" },
			{ Keycode.FIND, "Find" },
			{ Keycode.MUTE, "Mute" },
			{ Keycode.VOLUMEUP, "VolumeUp" },
			{ Keycode.VOLUMEDOWN, "VolumeDown" },
			{ Keycode.KP_COMMA, "Keypad  }," },
			{ Keycode.KP_EQUALSAS400, "Keypad, (AS400)" },
			{ Keycode.ALTERASE, "AltErase" },
			{ Keycode.SYSREQ, "SysReq" },
			{ Keycode.CANCEL, "Cancel" },
			{ Keycode.CLEAR, "Clear" },
			{ Keycode.PRIOR, "Prior" },
			{ Keycode.RETURN2, "Return" },
			{ Keycode.SEPARATOR, "Separator" },
			{ Keycode.OUT, "Out" },
			{ Keycode.OPER, "Oper" },
			{ Keycode.CLEARAGAIN, "Clear / Again" },
			{ Keycode.CRSEL, "CrSel" },
			{ Keycode.EXSEL, "ExSel" },
			{ Keycode.KP_00, "Keypad 00" },
			{ Keycode.KP_000, "Keypad 000" },
			{ Keycode.THOUSANDSSEPARATOR, "ThousandsSeparator" },
			{ Keycode.DECIMALSEPARATOR, "DecimalSeparator" },
			{ Keycode.CURRENCYUNIT, "CurrencyUnit" },
			{ Keycode.CURRENCYSUBUNIT, "CurrencySubUnit" },
			{ Keycode.KP_LEFTPAREN, "Keypad (" },
			{ Keycode.KP_RIGHTPAREN, "Keypad )" },
			{ Keycode.KP_LEFTBRACE, "Keypad {" },
			{ Keycode.KP_RIGHTBRACE, "Keypad }" },
			{ Keycode.KP_TAB, "Keypad Tab" },
			{ Keycode.KP_BACKSPACE, "Keypad Backspace" },
			{ Keycode.KP_A, "Keypad A" },
			{ Keycode.KP_B, "Keypad B" },
			{ Keycode.KP_C, "Keypad C" },
			{ Keycode.KP_D, "Keypad D" },
			{ Keycode.KP_E, "Keypad E" },
			{ Keycode.KP_F, "Keypad F" },
			{ Keycode.KP_XOR, "Keypad XOR" },
			{ Keycode.KP_POWER, "Keypad ^" },
			{ Keycode.KP_PERCENT, "Keypad %" },
			{ Keycode.KP_LESS, "Keypad <" },
			{ Keycode.KP_GREATER, "Keypad >" },
			{ Keycode.KP_AMPERSAND, "Keypad &" },
			{ Keycode.KP_DBLAMPERSAND, "Keypad &&" },
			{ Keycode.KP_VERTICALBAR, "Keypad |" },
			{ Keycode.KP_DBLVERTICALBAR, "Keypad ||" },
			{ Keycode.KP_COLON, "Keypad :" },
			{ Keycode.KP_HASH, "Keypad #" },
			{ Keycode.KP_SPACE, "Keypad Space" },
			{ Keycode.KP_AT, "Keypad @" },
			{ Keycode.KP_EXCLAM, "Keypad !" },
			{ Keycode.KP_MEMSTORE, "Keypad MemStore" },
			{ Keycode.KP_MEMRECALL, "Keypad MemRecall" },
			{ Keycode.KP_MEMCLEAR, "Keypad MemClear" },
			{ Keycode.KP_MEMADD, "Keypad MemAdd" },
			{ Keycode.KP_MEMSUBTRACT, "Keypad MemSubtract" },
			{ Keycode.KP_MEMMULTIPLY, "Keypad MemMultiply" },
			{ Keycode.KP_MEMDIVIDE, "Keypad MemDivide" },
			{ Keycode.KP_PLUSMINUS, "Keypad +/-" },
			{ Keycode.KP_CLEAR, "Keypad Clear" },
			{ Keycode.KP_CLEARENTRY, "Keypad ClearEntry" },
			{ Keycode.KP_BINARY, "Keypad Binary" },
			{ Keycode.KP_OCTAL, "Keypad Octal" },
			{ Keycode.KP_DECIMAL, "Keypad Decimal" },
			{ Keycode.KP_HEXADECIMAL, "Keypad Hexadecimal" },
			{ Keycode.LCTRL, "Left Ctrl" },
			{ Keycode.LSHIFT, "Left Shift" },
			{ Keycode.LALT, "Left Alt" },
			{ Keycode.LGUI, "Left GUI" },
			{ Keycode.RCTRL, "Right Ctrl" },
			{ Keycode.RSHIFT, "Right Shift" },
			{ Keycode.RALT, "Right Alt" },
			{ Keycode.RGUI, "Right GUI" },
			{ Keycode.MODE, "ModeSwitch" },
			{ Keycode.AUDIONEXT, "AudioNext" },
			{ Keycode.AUDIOPREV, "AudioPrev" },
			{ Keycode.AUDIOSTOP, "AudioStop" },
			{ Keycode.AUDIOPLAY, "AudioPlay" },
			{ Keycode.AUDIOMUTE, "AudioMute" },
			{ Keycode.MEDIASELECT, "MediaSelect" },
			{ Keycode.WWW, "WWW" },
			{ Keycode.MAIL, "Mail" },
			{ Keycode.CALCULATOR, "Calculator" },
			{ Keycode.COMPUTER, "Computer" },
			{ Keycode.AC_SEARCH, "AC Search" },
			{ Keycode.AC_HOME, "AC Home" },
			{ Keycode.AC_BACK, "AC Back" },
			{ Keycode.AC_FORWARD, "AC Forward" },
			{ Keycode.AC_STOP, "AC Stop" },
			{ Keycode.AC_REFRESH, "AC Refresh" },
			{ Keycode.AC_BOOKMARKS, "AC Bookmarks" },
			{ Keycode.BRIGHTNESSDOWN, "BrightnessDown" },
			{ Keycode.BRIGHTNESSUP, "BrightnessUp" },
			{ Keycode.DISPLAYSWITCH, "DisplaySwitch" },
			{ Keycode.KBDILLUMTOGGLE, "KBDIllumToggle" },
			{ Keycode.KBDILLUMDOWN, "KBDIllumDown" },
			{ Keycode.KBDILLUMUP, "KBDIllumUp" },
			{ Keycode.EJECT, "Eject" },
			{ Keycode.SLEEP, "Sleep" },
		};

		public static string DisplayString(Keycode k)
		{
			string ret;
			if (!KeyNames.TryGetValue(k, out ret))
				return k.ToString();

			return ret;
		}
	}
}
