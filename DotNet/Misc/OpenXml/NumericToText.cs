 private static string NumbersToText(long val)
{
	string res = RussianNumberText(val, true, "номер", "номера", "номеров");

	res = res.Replace("номеров", "");
	res = res.Replace("номера", "");
	res = res.Replace("номер", "");

	return res.Trim();
}

private static string DecimalToTextLong(decimal val)
{
	string res = RussianNumberText((long)val, true, "номер", "номера", "номеров");

	res = res.Replace("номеров", "");
	res = res.Replace("номера", "");
	res = res.Replace("номер", "");

	return res.Trim();
}

private static string DecimalToTextDecimal(decimal val)
{
	//string res1 = RussianNumberText((long)val, false, "целая", "целых", "целых");
	//string res2 = RussianNumberText((long)(val % 1.0m * 100), false, "сотая", "сотых", "сотых");

	//return res1.Trim() + " " + res2.Trim();

	string res = RussianNumberText((long)val, true, "номер", "номера", "номеров");

	res = res.Replace("номеров", "");
	res = res.Replace("номера", "");
	res = res.Replace("номер", "");

	return res.Trim() + " (" + ((int)(val % 1.0m * 100)).ToString("00") + ")";
}


private static string RussianNumberText(long val, bool male, string one, string two, string five)
{
	long n = val;
	int m = (int)(n % 1000);
	StringBuilder r = new StringBuilder();

	if (n == 0) r.Append("ноль ");
	if (n % 1000 != 0)
		r.Append(RusNumber.Str(m, male, one, two, five));
	else
		r.Append(five);

	n /= 1000;
	m = (int)(n % 1000);

	r.Insert(0, RusNumber.Str(m, false, "тысяча", "тысячи", "тысяч"));
	n /= 1000;
	m = (int)(n % 1000);

	r.Insert(0, RusNumber.Str(m, true, "миллион", "миллиона", "миллионов"));
	n /= 1000;
	m = (int)(n % 1000);

	r.Insert(0, RusNumber.Str(m, true, "миллиард", "миллиарда", "миллиардов"));
	n /= 1000;
	m = (int)(n % 1000);

	r.Insert(0, RusNumber.Str(m, true, "триллион", "триллиона", "триллионов"));
	n /= 1000;
	m = (int)(n % 1000);

	r.Insert(0, RusNumber.Str(m, true, "триллиард", "триллиарда", "триллиардов"));


	return r.ToString();
}








public class RusNumber
{
	private static string[] hunds =
	{
		"", "сто ", "двести ", "триста ", "четыреста ",
		"пятьсот ", "шестьсот ", "семьсот ", "восемьсот ", "девятьсот "
	};

	private static string[] tens =
	{
		"", "десять ", "двадцать ", "тридцать ", "сорок ", "пятьдесят ",
		"шестьдесят ", "семьдесят ", "восемьдесят ", "девяносто "
	};

	public static string Str(int val, bool male, string one, string two, string five)
	{
		string[] frac20 =
		{
			"", "один ", "два ", "три ", "четыре ", "пять ", "шесть ",
			"семь ", "восемь ", "девять ", "десять ", "одиннадцать ",
			"двенадцать ", "тринадцать ", "четырнадцать ", "пятнадцать ",
			"шестнадцать ", "семнадцать ", "восемнадцать ", "девятнадцать "
		};

		int num = val % 1000;
		if(0 == num) return "";
		if(num < 0) throw new ArgumentOutOfRangeException("val", "Параметр не может быть отрицательным");
		if(!male)
		{
			frac20[1] = "одна ";
			frac20[2] = "две ";
		}

		StringBuilder r = new StringBuilder(hunds[num / 100]);

		if(num % 100 < 20)
		{
			r.Append(frac20[num % 100]);
		}
		else
		{
			r.Append(tens[num % 100 / 10]);
			r.Append(frac20[num % 10]);
		}
		
		r.Append(Case(num, one, two, five));

		if(r.Length != 0) r.Append(" ");
		return r.ToString();
	}

	public static string Case(int val, string one, string two, string five)
	{
		int t=(val % 100 > 20) ? val % 10 : val % 20;

		switch (t)
		{
			case 1: return one;
			case 2: case 3: case 4: return two;
			default: return five;
		}
	}
};

struct CurrencyInfo
{
	public bool male;
	public string seniorOne, seniorTwo, seniorFive;
	public string juniorOne, juniorTwo, juniorFive;
};
 
public class RusCurrency
{
	private static HybridDictionary currencies = new HybridDictionary();

	static RusCurrency()
	{
		Register("RUR", true, "рубль", "рубля", "рублей", "копейка", "копейки", "копеек");
		Register("RUB", true, "рубль", "рубля", "рублей", "копейка", "копейки", "копеек");
		Register("EUR", true, "евро", "евро", "евро", "евроцент", "евроцента", "евроцентов");
		Register("USD", true, "доллар", "доллара", "долларов", "цент", "цента", "центов");

		Register("ALL", true, "лек", "лека", "леков", "киндарка", "киндарки", "киндарок");
		Register("AMD", true, "драм", "драма", "драмов", "лум", "лум", "лум");
		Register("ARS", true, "аргентинское песо", "аргентинских песо", "аргентинских песо", "сентаво", "сентаво", "сентаво");
		Register("ATS", true, "австрийский шиллинг", "австрийских шиллинга", "австрийских шиллингов", "грош", "гроша", "грошей");
		Register("AUD", true, "австралийский доллар", "австралийских доллара", "австралийских долларов", "цент", "цента", "центов");
		Register("AZM", true, "азербайджанский манат", "азербайджанских маната", "азербайджанских манатов", "гяпик", "гяпика", "гяпиков");
		Register("BEF", true, "бельгийский франк", "бельгийских франка", "бельгийских франков", "сантим", "сантима", "сантимов");
		Register("BGN", true, "лев", "лева", "левов", "стотинка", "стотинки", "стотинок");
		Register("BRL", true, "бразильский реал", "бразильских реала", "бразильских реалов", "сентаво", "сентаво", "сентаво");
		Register("BYR", true, "белорусский рубль", "белорусских рубля", "белорусских рублей", "копейка", "копейки", "копеек");
		Register("CAD", true, "канадский доллар", "канадских доллара", "канадских долларов", "цент", "цента", "центов");
		Register("CHF", true, "швейцарский франк", "швейцарских франка", "швейцарских франков", "сантим", "сантима", "сантимов");
		Register("CNY", true, "китайский юань", "китайских юаня", "китайских юаней", "фынь", "фыня", "фыней");
		Register("CYP", true, "кипрский фунт", "кипрских фунта", "кипрских фунтов", "цент", "цента", "центов");
		Register("CZK", false, "чешская крона", "чешских кроны", "чешских крон", "галирж", "галиржа", "галиржей");
		Register("DKK", false, "датская крона", "датских кроны", "датских крон", "эре", "эре", "эре");
		Register("EEK", false, "эстонская крона", "эстонских кроны", "эстонских крон", "сенти", "сенти", "сенти");
		Register("GBP", true, "фунт стерлингов", "фунта стерлингов", "фунтов стерлингов", "пенс", "пенса", "пенсов");
		Register("GEL", true, "грузинский лари", "грузинских лари", "грузинских лари", "тетри", "тетри", "тетри");
		Register("HKD", true, "гонконгский доллар", "гонконгских доллара", "гонконгских долларов", "цент", "цента", "центов");
		Register("HRK", false, "хорватская куна", "хорватских куны", "хорватских кун", "липа", "липы", "лип");
		Register("HUF", true, "венгерский форинт", "венгерских форинта", "венгерских форинтов", "филлер", "филлера", "филлеров");
		Register("INR", false, "индийская рупия", "индийские рупии", "индийских рупий", "пайс", "пайса", "пайсов");
		Register("ISK", false, "исландская крона", "исландских кроны", "исландских крон", "эре", "эре", "эре");
		Register("JPY", false, "иена", "иены", "иен", "сена", "сены", "сен");
		Register("KPW", false, "севернокорейская вона", "севернокорейских воны", "севернокорейских вон", "чон", "чоны", "чонов");
		Register("KRW", false, "южнокорейская вона", "южнокорейских воны", "южнокорейских вон", "чон", "чоны", "чонов");
		Register("KZT", true, "казахстанский тенге", "казахстанских тенге", "казахстанских тенге", "тиын", "тиына", "тиынов");
		Register("LAK", true, "кип", "кипа", "кипов", "ат", "ата", "атов");
		Register("LKR", false, "шри-ланкийская рупия", "шри-ланкийские рупии", "шри-ланкийских рупий", "цент", "цента", "центов");
		Register("LTL", true, "лит", "лита", "литов", "цент", "цента", "центов");
		Register("LUF", true, "люксембургский франк", "люксембургских франка", "люксембургских франков", "сантим", "сантима", "сантимов");
		Register("LVL", true, "лат", "лата", "латов", "сентим", "сентима", "сентимов");
		Register("MKD", true, "македонский динар", "македонских динара", "македонских динаров", "дени", "дени", "дени");
		Register("MTL", false, "мальтийская лира", "мальтийских лиры", "мальтийских лир", "сентим", "сентима", "сентимов");
		Register("MAD", true, "дихрам", "дихрама", "дихрамов", "сантим", "сантима", "сантимов");
		Register("MNT", true, "монгольский тугрик", "монгольских тугрика", "монгольских тугриков", "мунгу", "мунгу", "мунгу");
		Register("NLG", false, "нидерландский гульден", "нидерландских гульдена", "нидерландских гульденов", "цент", "цента", "центов");
		Register("NOK", false, "норвежская крона", "норвежских кроны", "норвежских крон", "эре", "эре", "эре");
		Register("PLN", true, "злотый", "злотых", "злотых", "грош", "гроша", "грошей");
		Register("ROL", true, "румынский лей", "румынских лей", "румынских лей", "бани", "бани", "бани");
		Register("SEK", false, "шведская крона", "шведских кроны", "шведских крон", "эре", "эре", "эре");
		Register("SGD", true, "сингапурский доллар", "сингапурских доллара", "сингапурских долларов", "цент", "цента", "центов");
		Register("SIT", true, "словенский толар", "словенских толара", "словенских толаров", "стотина", "стотины", "стотин");
		Register("SKK", false, "словацкая крона", "словацких кроны", "словацких крон", "геллер", "геллера", "геллеров");
		Register("SRD", true, "сингапурский доллар", "сингапурских доллара", "сингапурских долларов", "цент", "цента", "центов");
		Register("THB", true, "тайский бат", "тайских бата", "тайских бат", "сатанг", "сатанга", "сатангов");
		Register("TJS", true, "сомони", "сомони", "сомони", "дирам", "дирама", "дирамов");
		Register("TMM", true, "туркменский манат", "туркменских маната", "туркменских манатов", "тенге", "тенге", "тенге");
		Register("TMT", true, "туркменский манат", "туркменских маната", "туркменских манатов", "тенге", "тенге", "тенге");
		Register("TND", true, "тунисский динар", "тунисских динара", "тунисских динаров", "миллим", "миллима", "миллимов");
		Register("TRL", false, "турецкая лира", "турецких лиры", "турецких лир", "пиастр", "пиастра", "пиастров");
		Register("TRY", false, "турецкая лира", "турецких лиры", "турецких лир", "куруш", "куруша", "курушей");
		Register("UAH", false, "гривна", "гривны", "гривен", "цент", "цента", "центов");
		Register("YUM", true, "югославский динар", "югославских динара", "югославских динаров", "пара", "пара", "пара");
		Register("UZS", true, "сум", "сума", "сумов", "тийин", "тийина", "тийинов");
		Register("VND", true, "донг", "донга", "донгов", "су", "су", "су");
		Register("ZAR", true, "ранд", "ранда", "рандов", "цент", "цента", "центов");
	}

	public static void Register(string currency, bool male, 
		string seniorOne, string seniorTwo, string seniorFive,
		string juniorOne, string juniorTwo, string juniorFive)
	{
		CurrencyInfo info;
		info.male = male;
		info.seniorOne = seniorOne; info.seniorTwo = seniorTwo; info.seniorFive = seniorFive;
		info.juniorOne = juniorOne; info.juniorTwo = juniorTwo; info.juniorFive = juniorFive;
		currencies.Add(currency, info);
	}

	public static string Str(double val)
	{
		return Str(val, "RUR");
	}

	public static string Str(double val, string currency)
	{
		if(!currencies.Contains(currency)) 
			throw new ArgumentOutOfRangeException("currency", "Валюта \""+currency+"\" не зарегистрирована");
		
		CurrencyInfo info = (CurrencyInfo)currencies[currency];
		return Str(val, info.male, 
			info.seniorOne, info.seniorTwo, info.seniorFive,
			info.juniorOne, info.juniorTwo, info.juniorFive);
	}

	public static string Str(double val, bool male, 
		string seniorOne, string seniorTwo, string seniorFive,
		string juniorOne, string juniorTwo, string juniorFive)
	{
		bool minus = false;
		if(val < 0) { val = - val; minus = true; }

		int n = (int) val;
		int remainder = (int) (( val - n + 0.005 ) * 100);

		StringBuilder r = new StringBuilder();

		if(0 == n) r.Append("0 ");
		if(n % 1000 != 0)
			r.Append(RusNumber.Str(n, male, seniorOne, seniorTwo, seniorFive));
		else
			r.Append(seniorFive + " ");

		n /= 1000;
	 
		r.Insert(0, RusNumber.Str(n, false, "тысяча", "тысячи", "тысяч"));
		n /= 1000;
	 
		r.Insert(0, RusNumber.Str(n, true, "миллион", "миллиона", "миллионов"));
		n /= 1000;
	 
		r.Insert(0, RusNumber.Str(n, true, "миллиард", "миллиарда", "миллиардов"));
		n /= 1000;
	 
		r.Insert(0, RusNumber.Str(n, true, "триллион", "триллиона", "триллионов"));
		n /= 1000;
	 
		r.Insert(0, RusNumber.Str(n, true, "триллиард", "триллиарда", "триллиардов"));
		if(minus) r.Insert(0, "минус ");

		r.Append(remainder.ToString("00 "));
		r.Append(RusNumber.Case(remainder, juniorOne, juniorTwo, juniorFive));
	 
		//Делаем первую букву заглавной
		r[0] = char.ToUpper(r[0]);

		return r.ToString();
	}
};