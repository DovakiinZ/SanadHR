// Arabic → English (Latin) transliteration for auto-filling the English name field
// from the Arabic one. Deterministic and offline (no translation API): a dictionary
// of common Saudi/Arabic names for accuracy, with a letter-by-letter fallback for the rest.
//
// It is a best-effort *transliteration*, not a translation — good enough to pre-fill the
// Latin name, which the user can always tweak.

// Common given names + family names (most accurate path).
const DICTIONARY: Record<string, string> = {
  // Male given names
  "محمد": "Mohammed", "أحمد": "Ahmed", "احمد": "Ahmed", "عبدالله": "Abdullah", "عبد الله": "Abdullah",
  "عبدالرحمن": "Abdulrahman", "عبد الرحمن": "Abdulrahman", "عبدالعزيز": "Abdulaziz", "عبد العزيز": "Abdulaziz",
  "عبدالملك": "Abdulmalik", "عبدالمجيد": "Abdulmajeed", "عبداللطيف": "Abdullatif", "عبدالكريم": "Abdulkarim",
  "علي": "Ali", "خالد": "Khalid", "سعد": "Saad", "فهد": "Fahad", "سلطان": "Sultan", "ناصر": "Nasser",
  "سعود": "Saud", "عمر": "Omar", "يوسف": "Yousef", "إبراهيم": "Ibrahim", "ابراهيم": "Ibrahim",
  "ماجد": "Majed", "تركي": "Turki", "بندر": "Bandar", "نواف": "Nawaf", "مشعل": "Mishal", "فيصل": "Faisal",
  "سامي": "Sami", "وليد": "Waleed", "طارق": "Tariq", "ريان": "Rayan", "زياد": "Ziyad", "بدر": "Badr",
  "ياسر": "Yasser", "عادل": "Adel", "حسن": "Hassan", "حسين": "Hussain", "صالح": "Saleh", "عثمان": "Othman",
  "مازن": "Mazen", "أنس": "Anas", "معاذ": "Muath", "حمد": "Hamad", "راكان": "Rakan", "عبدالإله": "Abdulelah",
  "سلمان": "Salman", "سلطانة": "Sultana", "نايف": "Naif", "ثامر": "Thamer", "مساعد": "Musaed",
  "عبدالمحسن": "Abdulmohsen", "عبدالوهاب": "Abdulwahab", "زيد": "Zaid", "صقر": "Saqr", "غازي": "Ghazi",
  // Name particles
  "بن": "bin", "ابن": "ibn", "بنت": "bint", "آل": "Al", "أبو": "Abu", "ابو": "Abu",
  // Female given names
  "فاطمة": "Fatimah", "نورة": "Noura", "نوره": "Noura", "سارة": "Sarah", "ساره": "Sarah", "ريم": "Reem",
  "هند": "Hind", "العنود": "Alanoud", "لمياء": "Lamia", "مها": "Maha", "أمل": "Amal", "منيرة": "Munirah",
  "الجوهرة": "Aljawharah", "عائشة": "Aisha", "خلود": "Khulood", "رنا": "Rana", "دانة": "Dana", "جواهر": "Jawaher",
  // Common family names (tribal)
  "العتيبي": "Alotaibi", "القحطاني": "Alqahtani", "الغامدي": "Alghamdi", "الشهري": "Alshehri",
  "الدوسري": "Aldosari", "الحربي": "Alharbi", "المالكي": "Almalki", "الزهراني": "Alzahrani",
  "الشمري": "Alshammari", "العنزي": "Alanazi", "المطيري": "Almutairi", "السبيعي": "Alsubaie",
  "الرشيدي": "Alrashidi", "البقمي": "Albaqami", "الشيخ": "Alsheikh", "السهلي": "Alsahli",
  "الجهني": "Aljuhani", "البلوي": "Albalawi", "العمري": "Alomari", "الخالدي": "Alkhalidi",
  "الفيفي": "Alfaifi", "الثبيتي": "Althubaiti", "الحارثي": "Alharthi", "المحمدي": "Almohammadi",
  "الأحمدي": "Alahmadi", "العمر": "Alomar", "الدخيل": "Aldakhil", "القرني": "Alqarni",
};

// Diacritics / marks to strip.
const STRIP = /[ً-ْٰـ]/g; // tanween, harakat, dagger alif, tatweel

// Single-letter map (fallback). Digraphs first via the map keys being single chars,
// but multi-char outputs (sh, kh, …) are produced directly.
const LETTERS: Record<string, string> = {
  "ا": "a", "أ": "a", "إ": "i", "آ": "aa", "ب": "b", "ت": "t", "ث": "th", "ج": "j",
  "ح": "h", "خ": "kh", "د": "d", "ذ": "dh", "ر": "r", "ز": "z", "س": "s", "ش": "sh",
  "ص": "s", "ض": "d", "ط": "t", "ظ": "z", "ع": "a", "غ": "gh", "ف": "f", "ق": "q",
  "ك": "k", "ل": "l", "م": "m", "ن": "n", "ه": "h", "و": "w", "ي": "y", "ى": "a",
  "ة": "a", "ء": "", "ئ": "y", "ؤ": "w", "پ": "p", "چ": "ch", " گ": "g", "ڤ": "v",
  "٠": "0", "١": "1", "٢": "2", "٣": "3", "٤": "4", "٥": "5", "٦": "6", "٧": "7", "٨": "8", "٩": "9",
};

function titleCase(w: string): string {
  return w.length ? w[0].toUpperCase() + w.slice(1) : w;
}

function transliterateWord(raw: string): string {
  const word = raw.replace(STRIP, "").trim();
  if (!word) return "";
  if (DICTIONARY[word]) return DICTIONARY[word];

  // "ال" (definite article / family prefix) → "Al"
  let rest = word;
  let prefix = "";
  if (word.startsWith("ال") && word.length > 2) {
    prefix = "Al";
    rest = word.slice(2);
    if (DICTIONARY["ال" + rest]) return DICTIONARY["ال" + rest];
  }

  let out = "";
  for (const ch of rest) {
    out += LETTERS[ch] ?? (/[a-zA-Z0-9]/.test(ch) ? ch : "");
  }
  return prefix ? prefix + out : titleCase(out);
}

/**
 * Transliterate an Arabic string to a Latin (English) approximation, word by word.
 * Returns "" for empty/whitespace-only input.
 */
export function transliterateArabic(input: string): string {
  if (!input) return "";
  // If the text has no Arabic letters, leave it as-is (user may have typed Latin already).
  if (!/[؀-ۿ]/.test(input)) return input;
  return input
    .trim()
    .split(/\s+/)
    .map(transliterateWord)
    .filter(Boolean)
    .join(" ");
}
