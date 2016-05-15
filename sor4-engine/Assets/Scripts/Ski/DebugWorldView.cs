using System;
using UnityEngine;
using System.Collections.Generic;
using RetroBread;
using RetroBread.Network;
using System.Linq;


public class DebugWorldView:View<WorldModel>{
	
	// The views
	public GameObject[] skierViews;
	bool[] skierCollisionSoundActivated;

	// leaderboard times
	int[] finishedSkiers;
	string[] skiersNames;

	// Common material used by all views
	static Material meshesMaterial;

	const float lerpFrozenTimeFactor = 0.05f;
	const float lerpTimeFactor = 0.2f; 
	const float lerpAngleFactor = 0.1f;
	
	static GameObject skierPrefab;
	static int rideAnimHash;
	static int fallAnimHash;
	static int crashAnimHash;

	static AudioClip[] painClips;
	static AudioClip failClip;

	static GameObject arrowsToPointNextFlag;


	private static string[] randomNames = {"Vordnys", "Ern", "Ytino", "Tasina", "Age'ad", "Radodel", "Perd", "Est'und", "Ineum", "Tiaough", "Isquat", "Ighttai", "Osyer", "Blichad", "Warbur", "Burray", "Hatont", "Kayuzocu", "Esstai", "Nyight", "Chaine", "Ougherad", "Ghaves", "Umhat", "Estmard", "Perup", "Irt", "Vesmor", "Darech", "Mosir", "Rojaby", "Netad", "Ormath", "Sered", "Hatlor", "Chrould", "Stielt", "Tinradu", "Goum", "Garasden", "Urnard", "Orothi", "Rilgha", "Ingt", "Morjrod", "Vesburdyn", "Say'mosa", "Vynet", "Ryncheer", "Ang'thero", "Tufaku", "Ziaz", "Verorma", "Ennrad", "Shyald", "Rakeng", "Delald", "Tonend", "Cluightiss", "Artler", "Erale", "Umpol", "Oughlves", "Dan'angu", "Ceryk", "Belan", "Devkin", "Ikalo", "Tiarad", "Inaw", "Omentin", "Uiro", "Yhini", "Achdeni", "Yecodu", "Throchether", "Gehox", "Uane", "Rulot", "Ar'gar", "Warool'd", "Hontas", "Darddra", "Rodock", "Tainy", "Vesunt-dyn", "Lyeas", "Thed", "Stayn", "Oldih", "Achest", "Epera", "Nuhayr", "Gaden", "Puredu", "Bantia", "Et'atha", "Ildage", "Enthiss", "Slewor", "Onysu", "Tant", "Den'ori", "Ogara", "Tinikim", "Itackiss", "Reaghech", "Burray", "Eld'dra", "Leend", "Oemi", "Mew", "Ghaorskel", "Hedab", "Yerm", "Serh", "Luxyhi", "Lobyl", "Chrekim", "Lebegy", "Toraro", "Nagala", "Ashoas", "Em'echa", "Looughche", "Kimtai", "Seromy", "Yray", "Tinrodest", "Yrano", "Sayden", "Ychao", "Dian", "Ildomwar", "Shydan", "Lor'old", "Toathina", "Chegh", "Ustald", "Ir'ech", "Ackhate", "Umanshy", "Askela", "Ild'queu", "Ir'buro", "Styanina", "Quehon", "Lytai", "Uskdra", "Engtur", "Ghaengo", "Eorme", "Os'hin'einn", "Laranche", "Nysoki", "Jaythust", "Paementh", "Hiashiss", "Waresturn", "Englye", "Bierald", "Emios", "Vorrak", "Kel'elda", "Moruas", "Sulsh", "Chad", "Kinm", "Ing'est", "Enr", "Tan'tia", "Oughdenves", "Cerranser", "Untris", "Banash", "Imhon", "Kaltia", "Wekavi", "Et'rak", "As'enge", "Ad'inga", "Ray-areld", "Sefoj", "Aughusaya", "Beestnal", "Danrothchehat", "Itzray", "Diem", "Sayl", "Hatrak", "Obure", "Sayrtkin", "Vetare", "Sermit", "Miliy", "Totanwor", "Kim'as", "Leiltmor", "Puser", "Samjard", "Nystund", "Pol'eto", "Sniad", "Jagldbur", "Dadraend", "Nighssul", "Rilom", "Imril", "Emz", "Hyxoh", "Asskelshy", "Rot", "Ir'lye", "Oacka", "Pereld", "Attor", "Rilat", "Cias", "Mosen", "Burech", "Umque", "Eoldu", "Ildyser", "Dasayen", "Emth", "Uraye", "Kekov", "Necnal", "Seetim", "Keleld", "Rylasa", "Atrak", "Chenal", "Ghaurn", "Per'urn", "Biph", "Danbane", "Muramarui", "Yoshisen", "Shirobashi", "Fuku", "Keifuru", "Asayuki", "Mura", "Kitasaka", "Yamaki", "Zakiwa", "Gintaka", "Watafuru", "Kamizawa", "Motomatsu", "Ueshiro", "Uematsu", "Morihara", "Asaue", "Suzushima", "Sakizen", "Toyowa", "Takaki", "Furugin", "Hontoyo", "Kawahana", "Zawaoo", "Toto", "Aki", "Senmarui", "Ichishima", "Kawahira", "Kuchitake", "Ikita", "Harayama", "Fujihashi", "Kuchikei", "Nabenaka", "Fujigin", "Furubashi", "Oosawa", "Shitataka", "Furufuru", "Haruken", "Morishiro", "Totoyo", "Shimataka", "Kisuzu", "Kuromatsu", "Shiroyama", "Marui", "Kuroken", "Nishi", "Wara", "Keiyoshi", "Nakaoo", "Hiramori", "Kitasawa", "Tasuzu", "Ooko", "Bashiao", "Suzuhana", "Aoo", "Toyokin", "Asasen", "Koharu", "Kiwara", "Okawara", "Nota", "Aowata", "Harazaki", "Yamayoshi", "Ooshita", "Kuro", "Dao", "Hoshimiya", "Kinkuro", "Honken", "Nishiyuki", "Zen", "Furutaka", "Akaka", "Nishisaka", "Tahashi", "Kinhara", "Kamiue", "Too", "Omiya", "Yoshisuzu", "Nabemori", "Nishihira", "Sakaoka", "Sensen", "Haruyuki", "Ochi", "Hon", "Haratoyo", "Yamamarui", "Kamifuku", "Oto", "Kawazawa", "Akikuro", "Hanayoshi", "Kin", "Kihashi", "Matsusa", "Haraki", "Kamikawa", "Ichi", "Ken", "Yuki", "Ginkami", "Ginmarui", "Sen", "Okamarui", "Ginyuki", "Uehira", "Fukutaka", "Nishifuku", "Eulasti", "Tyisri", "Aelollo", "Silastical", "Istysti", "Phyalaecal", "Idaria", "Heosristi", "Ilosudru", "Eustydil", "Istysuir", "Ila", "Haelae", "Hailly", "Ririthria", "Reuli", "Seoralria", "Silaeriam", "Aduelis", "Tyaedarin", "Risty", "Inan", "Eulargue", "Pheulae", "Ailillo", "Illynu", "Reolaeru", "Ilasu", "Phastyria", "Saelostiphos", "Thiadallo", "Aesristi", "Eoduenura", "Bliallysti", "Eurithrgue", "Pharithnu", "Pheully", "Haesty", "Rialollo", "Chreola", "Iarithriadil", "Eulaesura", "Athe", "Eoduesuir", "Seosurn", "Tyiduergue", "Asur", "Iarallo", "Phistyriarap", "Ialiria", "Yaloriacal", "Sirinu", "Aina", "Yasurnup", "Iduesum", "Hiadairiara", "Brisur", "Riralp", "Pheuralsu", "Aelora", "Ida", "Yala", "Tyeostyru", "Ithe", "Aellystip", "Aeda", "Tyeulidru", "Kaelanup", "Eoristi", "Saeral", "Eudaillocal", "Astyrin", "Phalaerap", "Yalarin", "Tyeudueria", "Aelidru", "Eulaeria", "Aedairia", "Tyiariria", "Phiadaideu", "Ilaenura", "Pharithstip", "Haisurnu", "Saedaidil", "Rilaphos", "Sweolae", "Adadru", "Phiarinudru", "Anariacal", "Tyaistystim", "Asurriarin", "Iduephos", "Ila", "Ilical", "Aenargue", "Seoral", "Phathestiru", "Aistydil", "Dyathe", "Ithe", "Isrinu", "Illy", "Illyphos", "Laililis", "Ririllo", "Aerin", "Aralra", "Phyaralnura", "Aelly", "Pheoduello", "Aisurllo", "Yadue", "Aerithsti", "Eurithsulis", "Raisurria", "Eusristi", "Anadeu", "Syalo", "Haelasusuir", "Darem", "Vesleri", "Wosam", "Queest", "Tinerr", "Hyer", "Samalda", "Hitor", "Trerod", "Oldsuly", "Rodiw", "Endis", "Estzver", "Oldmosy", "Ancdel", "Broque", "Tanik", "Garnther", "Lar", "Sulsshy", "Trim", "Chight", "Yias", "Umrper", "Ding", "Sydan", "Undiq", "Howar", "Lekel", "Cerare", "Tiusk", "Snosam", "Elmrris", "Ashdeny", "Enest", "Eldurt", "Burisse", "Ormur", "Tasiss", "Achati", "Erint", "Ildic", "Aughing", "Skelkale", "Threkel", "Keler", "Elmcaugh", "Rothquee", "Rakntin", "Raknpol", "Eldlye", "Shishy", "Imow", "Irine", "Yerod", "Elmati", "Worttur", "Quetia", "Tasaph", "Isndan", "Delir", "Quacdel", "Elmken", "Banard", "Eldyw", "Tonlyeo", "Omsenth", "Estald", "Duhon", "Warer", "Phidra", "Gashy", "Lilye", "Acham", "Clyaw", "Ackelde", "Anemo", "Turirr", "Rothenth", "Ildisi", "Hinyunt", "Dariw", "Ranhsam", "Gion", "Thahin", "Wier", "Rothaughi", "Garsay", "Danaugh", "Itor", "Saytis", "Mosann", "Marak", "Oughlkel", "Bantino", "Erumo", "Burdel", "Lloden", "Vesrisa", "Bashy", "Rynlgar", "Aldvory", "Lerrayu", "Emoldu", "Noach", "Echat", "Aughril", "Undaq", "Soum", "Liaw", "Tiaoh", "Asytas", "Delen", "Inedyn", "Belerd", "Elmcmos", "Therkray", "Adup", "Rodrril", "Grorg", "Glulfpolph", "Noo", "Bormhead", "Thooznit", "Bridiotthimble", "Moofthong", "Hulf", "Klorklump", "Fumphong", "Clokforon", "Ponkum", "Boogum", "Glidiot", "Fum", "Grormface", "Wuzz", "Gonkdip", "Dunuzz", "Kloof", "Wubpoof", "Kungjug", "Gumborm", "Bok", "Blorm", "Numb", "Polphcloof", "Guzzhoo", "Klormkoron", "Hidiotult", "Fub", "Korkgoog", "Dorgface", "Fumphknuckle", "Boofthump", "Domphfomph", "Dulfclown", "Glung", "Goronball", "Multthimble", "Jolph", "Boofoot", "Thulf", "Dumb", "Gub", "Fomphjoo", "Flidiot", "Punph", "Flidiot", "Doobgridiot", "Thoo", "Gumloaf", "Froronbrult", "Boobum", "Form", "Poltung", "Frongmong", "Koozfinger", "Bruzz", "Funtgrunph", "Clumphtwit", "Glungmolph", "Klunbhead", "Poork", "Gelchelch", "Mormoof", "Folphdelch", "Bog", "Formghuck", "Wuckclolph", "Clumbfong", "Nungbone", "Wong", "Mug", "Mobjuck", "Folph", "Pub", "Gug", "Flonk", "Blugface", "Num", "Form", "Blog", "Gronkgorm", "Noobghun", "Glelchwolt", "Guckgun", "Gumphnum", "Blump", "Gunphnolph", "Flooffrorg", "Nomph", "Blult", "Guck", "Pum", "Bulfjoof", "Gorongidiot", "Klulf", "Borgnoog", "Glolph", "Fook", "Golt", "Dokflork", "Worgolph", "Pumphgunb", "Grolph", "Clelchfidiot", "Kuntloaf", "Pooftwit", "Puzzwipe", "Munboog", "Pogorg", "Nugguzz", "Fongskull", "Gunph", "Pulf", "Pookwomph", "Woogforon", "Ghog", "Doofpuff", "Airnumb", "Wadwipefoot", "Knocksneeze", "Clodgoof", "Twerpsnark", "Ballpuff", "Dooffacetwit", "Lumpmeat", "Twerpfinger", "Fumblethimble", "Facefumbledoof", "Wipegoof", "Clodfacenumb", "Wadtwerp", "Thimblehead", "Knockpuff", "Airmuckface", "Lumpair", "Headgrumble", "Cheesesnark", "Facehead", "Clodtwit", "Bonewad", "Knockgoof", "Faceclot", "Twerpfumble", "Thimbleloaf", "Clodtwerp", "Bonedork", "Lunkface", "Cornwipetwerp", "Faceknocker", "Dorkbump", "Wipeknock", "Doltcheese", "Nitmunch", "Knockfoot", "Cheeseskullmuck", "Dipdoof", "Pinpin", "Ankledork", "Goofnit", "Puffball", "Doofankle", "Ankledumb", "Thimbleclodmunch", "Anklepin", "Cornair", "Goofdoofdip", "Thimbledorkbumble", "Knockerfinger", "Pinknuckle", "Sneezeairmuck", "Ankletwit", "Knucklehead", "Skullknocker", "Facetwit", "Wadbump", "Clotbeef", "Bumplump", "Munchbumair", "Doltgrumbleair", "Knuckleknock", "Pinlunk", "Bumpclot", "Munchdoof", "Cheesetwit", "Anklegoof", "Lumplump", "Wipeface", "Twerpgoof", "Twerpbeef", "Clotwimp", "Lunkdork", "Lumpdip", "Lunknit", "Cheeseclod", "Sneezeclot", "Clodwipe", "Fumbleface", "Puffankle", "Thimblethimble", "Bumppuff", "Nitknocker", "Corndumb", "Grumbleclown", "Clotfoot", "Meathead", "Fingerair", "Clodknocker", "Footnumb", "Goofthimble", "Fingerclod", "Clotgoof", "Pinmuckloaf", "Beefbumble", "Wadfumble", "Footballbone", "Ballmunchdumb", "Fingerwipe", "Cheeseair", "Twerpfingerball", "Ankleskullpin", "Snarkfumble", "Headgrumbleclot", "Wimpthimble", "Faceclot", "Sneezedip", "Dippin", "Knockwad", "Fingergoof", "Sneezefoot", "Skullmeat", "Nitdipwad", "Munchface", "Bumbumble", "Knockwimpdip", "Knucklegrumble", "Kalst", "Otone", "Snerr", "Lleent", "Epero", "Sulk", "Oemo", "Yees", "Zoib", "Itiay", "Yiar", "Roir", "Undi", "Uskrr", "Isv", "Raell", "Hind", "Pet", "Belf", "Awd", "Atine", "Ransh", "Ehiny", "Lyey", "Zhaus", "Etheri", "Arode", "Adt", "Bosh", "Thrert", "Breuv", "Athera", "Torl", "Yryna", "Voz", "Chent", "Hond", "Idrau", "Teup", "Smiph", "Yaen", "Echt", "Daess", "Otasa", "Aesti", "Miert", "Urayo", "Eendo", "Chelt", "Threr", "Throy", "Ueno", "Emoro", "Rodr", "Agell", "Sloond", "Garn", "Strayr", "Rael", "Teyrr", "Orz", "Ealei", "Ehiny", "Etrt", "Heinn", "Niel", "Lerr", "Oatho", "Iomy", "Verc", "Cream", "Ormll", "Reul", "Usayi", "Ighay", "Anr", "Scheus", "Fac", "Ethery", "Ardd", "Rayr", "Sulst", "Straes", "Umoso", "Teh", "Riath", "Gand", "Choiss", "Rhoes", "Zayth", "Slois", "Steald", "Ytori", "Mogh", "Eiai", "Photh", "Theenn", "Itona", "Oomo", "Essk", "Vesk", "Naess", "Tony", "Aleld", "Agek", "Kaw", "Main", "Hinh", "Oldl", "Perv", "Oene", "Reyt", "Meis", "Zhos", "Polrr", "Awari", "Waind", "Saim", "Uraye"};



	// local copy of last state's map, to identify map changes
	int[] lastKnownMap;

	static DebugWorldView(){
		// setup imutable stuff
		meshesMaterial = new Material(Shader.Find("Sprites/Default"));

		skierPrefab = Resources.Load("skier") as GameObject;
		rideAnimHash = Animator.StringToHash("Ride");
		fallAnimHash = Animator.StringToHash("Fall");
		crashAnimHash = Animator.StringToHash("Crash");

		arrowsToPointNextFlag = GameObject.Find("nextFlagPointingArrows");
		arrowsToPointNextFlag.SetActive(false);

		painClips = new AudioClip[10];
		for (int i = 0 ; i < 10 ; ++i){
			painClips[i] = Resources.Load("sound/WS_pain_" + (i+1)) as AudioClip;
		}

		failClip = Resources.Load("sound/WS_penalty") as AudioClip;
	}


	public DebugWorldView(WorldModel world){
		// Allocate memory for views arrays
		skierViews = new GameObject[WorldModel.MaxPlayers];
		skierCollisionSoundActivated = new bool[WorldModel.MaxPlayers];
		finishedSkiers = new int[WorldModel.MaxPlayers];
		skiersNames = new string[WorldModel.MaxPlayers];
	}



	protected override void Update(WorldModel model, float deltaTime){

		UpdateSkiers(model, deltaTime);

		// check if next flag is outside screen
		int playerId = 0;
		if (StateManager.Instance.IsNetworked) {
			playerId = NetworkCenter.Instance.GetPlayerNumber();
			if (playerId < 0 || playerId >= model.skiers.Length)
				return;
		}

		SkierModel mySkier = model.skiers[playerId];

		WorldObject flag = WorldObjects.GetNextFlagForSkier(mySkier);
		if (flag == null) return;
		Vector3 flagScreenPos = new Vector3((float)(flag.isRight ? flag.x1 : flag.x2), 0.0f, (float)flag.y);
		flagScreenPos = Camera.main.WorldToScreenPoint(flagScreenPos);
		flagScreenPos = new Vector3(flagScreenPos.x / Camera.main.pixelWidth, flagScreenPos.y / Camera.main.pixelHeight, 2.0f);
		bool setActive = false;
		float distanceAway = 0.0f;
		if (flagScreenPos.x < 0 && flag.isRight) {
			distanceAway = -flagScreenPos.x;
			arrowsToPointNextFlag.transform.localEulerAngles = new Vector3(0, 180, 0);
			Vector3 arrowsPosition = Camera.main.ScreenToWorldPoint(new Vector3(0.00f * Camera.main.pixelWidth, Mathf.Clamp(flagScreenPos.y, 0.1f, 0.95f) * Camera.main.pixelHeight, flagScreenPos.z));
			arrowsToPointNextFlag.transform.position = arrowsPosition;
			setActive = true;
		}else if (flagScreenPos.x > 1 && !flag.isRight) {
			distanceAway = flagScreenPos.x - 1;
			arrowsToPointNextFlag.transform.localEulerAngles = new Vector3(0, 0, 0);
			Vector3 arrowsPosition = Camera.main.ScreenToWorldPoint(new Vector3(1.0f * Camera.main.pixelWidth, Mathf.Clamp(flagScreenPos.y, 0.1f, 0.95f) * Camera.main.pixelHeight, flagScreenPos.z));
			arrowsToPointNextFlag.transform.position = arrowsPosition;
			setActive = true;
		}
		if (setActive) {
			arrowsToPointNextFlag.SetActive(true);
			flag.ApplyColorToFlagArros(arrowsToPointNextFlag);
			float scale = 0.1f - 0.08f * (1 - Mathf.Min(distanceAway, 3) / 3) + Mathf.Max(0.8f - flagScreenPos.y,0) * 0.25f;
			scale *= 2.5f;
			arrowsToPointNextFlag.transform.localScale = new Vector3(scale, scale, scale);
		}
		arrowsToPointNextFlag.SetActive(setActive);

		UpdateLeaderboard(model);
	}


	private string GetSkierLeaderboardName(int skierId){
		if (skiersNames[skierId] == null) {
			if (StateManager.Instance.IsNetworked) {
				NetworkPlayerData data;
				data = NetworkCenter.Instance.GetPlayerData((uint) skierId);
				if (data == null) return null;
				skiersNames[skierId] = data.playerName;
			} else {
				if (skierId == 0) {
					skiersNames[skierId] = GuiMenus.Instance.nickname;
				}else {
					int rnd = UnityEngine.Random.Range (0, 15);
					if (rnd == 2) {
						skiersNames [skierId] = GuiMenus.defaultNickname + UnityEngine.Random.Range (0, 999999);
					} else if (rnd < 2) {
						skiersNames [skierId] = GuiMenus.defaultNickname + UnityEngine.Random.Range (0, 99999999);
					} else if (rnd < 6){
						skiersNames [skierId] = GuiMenus.defaultNickname + UnityEngine.Random.Range (0, 129999999);
					}else {
						skiersNames[skierId] = randomNames[UnityEngine.Random.Range(0, randomNames.Length)];
					}
				}
			}
		}
		return skiersNames[skierId];
	}


	private void UpdateLeaderboard(WorldModel model){
		GameObject leaderboard = GuiMenus.Instance.leaderboardObject;
		KeyValuePair<int, float>[] skiersYs = new KeyValuePair<int, float>[model.skiers.Count()];
		float order;
		for (int i = 0 ; i < skiersYs.Count() ; ++i){
			if (finishedSkiers[i] != 0) order = -999990 + finishedSkiers[i];
			else if(model.skiers[i] == null) order = 999;
			else order = (float)model.skiers[i].y;
			skiersYs[i] = new KeyValuePair<int, float>(i, order);
		}
		skiersYs = skiersYs.OrderBy(x=>x.Value).ToArray<KeyValuePair<int, float>>();
	
		Transform leadTransf = leaderboard.transform;
		UnityEngine.UI.Text textItem;
		string entryString;
		int ownPlayerNumber = StateManager.Instance.IsNetworked ? NetworkCenter.Instance.GetPlayerNumber() : 0;
		int orderedId;
		int position = 1;
		int winnerFrameNum = -1;
		for (int i = 0 ; i < WorldModel.MaxPlayers ; ++i){
			textItem = leadTransf.GetChild(i).gameObject.GetComponent<UnityEngine.UI.Text>();
			textItem.enabled = i < skiersYs.Count() && skiersYs[i].Value != 999;
			if (textItem.enabled) {
				orderedId = skiersYs[i].Key;
				entryString = GetSkierLeaderboardName(orderedId);
				textItem.enabled = entryString != null;
				if (textItem.enabled){
					if (finishedSkiers[orderedId] == 0 && skiersYs[i].Value <= -WorldObjects.finalGoalDistance) {
						int frameNum = (int) Mathf.Max((int)StateManager.state.Keyframe - (int)WorldController.framesToStart, 0.0f);
						finishedSkiers[orderedId] = frameNum;
					}
					if (finishedSkiers[orderedId] != 0) {
						int frameNum = finishedSkiers[orderedId];
						float currentTime;
						if (winnerFrameNum > 0) {
							currentTime = StateManager.Instance.UpdateRate * (frameNum - winnerFrameNum);
							entryString = "+" + ClockCounter.FloatToTime(currentTime, "#0:00") + " - " + skiersNames [orderedId];
						} else {
							currentTime = StateManager.Instance.UpdateRate * frameNum;
							entryString = ClockCounter.FloatToTime (currentTime, "#0:00.00") + " - " + skiersNames [orderedId];
							winnerFrameNum = frameNum;
						}
					} else {
						entryString = position + ". " + entryString;
					}
					textItem.text = entryString;
					textItem.color = orderedId == ownPlayerNumber ? Color.yellow : Color.white;

					if (orderedId == ownPlayerNumber) {
						GuiMenus.Instance.playerPosition = position;
					}

					++position;
				}
			}
		}
		leaderboard.SetActive(position > 1);

		string winnerName = skiersNames[skiersYs[0].Key];
		if (winnerName != null) {
			GuiMenus.Instance.loserObj.transform.GetChild(0).GetComponent<UnityEngine.UI.Text>().text = winnerName + " won";
		}

	}


	private void UpdateSkiers(WorldModel model, float deltaTime){
		for (int i = 0 ; i < model.skiers.Length ; ++i){
			// update creation or destruction
			SkierModel skierModel = model.skiers[i];
			if (skierModel != null && skierViews[i] == null) {
				bool own = false;
				if (StateManager.Instance.IsNetworked) {
					own = NetworkCenter.Instance.GetPlayerNumber () == i;
				} else {
					own = i == 0;
				}
				skierViews[i] = CreateSkier(own);

				float targetAngle = skierModel.targetVelY == 0 ? -Mathf.PI * 0.5f : (float)Mathf.Atan2((float)skierModel.targetVelX, (float)skierModel.targetVelY);
				targetAngle = targetAngle * Mathf.Rad2Deg;
				skierViews[i].transform.localEulerAngles = new Vector3(0, targetAngle, 0);

			}else if (skierModel == null && skierViews[i] != null){
				GameObject.Destroy(skierViews[i]);
				skierViews[i] = null;
			}
			GameObject skierView = skierViews[i];

			// update position
			if (skierView != null){

				// update position
				float yPos = (skierModel.fallenTimer > 0 || skierModel.frozenTimer > 0) ? 0.32f : -0.3f;
				Vector3 targetPos = new Vector3((float)skierModel.x, yPos, (float)skierModel.y);
				UpdatePosition(skierModel, skierView, targetPos);
				skierView.transform.position = new Vector3 (skierView.transform.position.x, yPos, skierView.transform.position.z);

				// update angle
				float targetAngle = skierModel.targetVelY == 0 ? -Mathf.PI * 0.5f : (float)Mathf.Atan2((float)skierModel.targetVelX, (float)skierModel.targetVelY);
				targetAngle = targetAngle * Mathf.Rad2Deg;
				float originalAngle = skierView.transform.localEulerAngles.y;
				normalizeAnglesDifference(ref targetAngle, originalAngle);
				targetAngle = Mathf.Lerp(originalAngle, targetAngle, lerpAngleFactor);
				skierView.transform.localEulerAngles = new Vector3(0, targetAngle, 0);

				// update animation
				Animator animator = skierView.GetComponent<Animator>();
				if (animator != null) {
					// update moving animation

					if (skierModel.fallenTimer == 0 && skierModel.frozenTimer == 0){

						animator.Play(rideAnimHash);
						float frictionRate = Mathf.Abs((float)skierModel.friction);
						float blendFactor = (float)(skierModel.friction) * 2.0f;
						Mathf.Clamp (blendFactor, -1, 1);
						blendFactor = blendFactor * 0.5f + 0.5f;
						animator.SetFloat ("Blend", blendFactor);
						animator.speed = new Vector2 ((float)skierModel.velX, (float)skierModel.velY).magnitude;

						ParticleSystem particles = skierView.GetComponentInChildren<ParticleSystem>();
						if (particles != null) {
							particles.enableEmission = true;
							particles.emissionRate = 1 + animator.speed * 20 + frictionRate * 329;
							particles.transform.localEulerAngles = new Vector3(30 + (1 - frictionRate) * 60, 90, 0);
							particles.transform.localPosition = new Vector3(-0.95f, -0.75f, -0.26f);
						}

						// Audio
						AudioSource[] audio = skierView.GetComponents<AudioSource>();
						if (audio != null){
							if(!audio[0].isPlaying){
								audio[0].Play();
							}
							if(!audio[1].isPlaying){
								audio[1].Play();
							}
							audio[0].volume = animator.speed * animator.speed * 0.3f;
							audio[1].volume = frictionRate * 0.4f;
							skierCollisionSoundActivated[i] = false;
						}

					}else {
						if (animator.GetCurrentAnimatorStateInfo(0).shortNameHash != crashAnimHash && animator.GetCurrentAnimatorStateInfo(0).shortNameHash != fallAnimHash){
							bool pickCrash = skierModel.frozenTimer != 0 || UnityEngine.Random.Range(0,4) == 1;
							animator.Play(pickCrash ? crashAnimHash : fallAnimHash);
							skierView.transform.localEulerAngles = new Vector3(0, 180, 0);
							animator.speed = pickCrash ? 1.5f : 1.0f;//(pickCrash && skierModel.frozenTimer != 0) ? 1.25f : 1.0f;
						}

						ParticleSystem particles = skierView.GetComponentInChildren<ParticleSystem>();
						if (particles != null) {
							particles.enableEmission = false;
							//particles.transform.localPosition = new Vector3(particles.transform.localPosition.x, -999, particles.transform.localPosition.z);
							particles.transform.position = new Vector3(99999, -99999, 99999);
						}

						// Audio
						AudioSource[] audio = skierView.GetComponents<AudioSource>();
						if (audio != null) {
							if (!skierCollisionSoundActivated[i]){
								audio[0].Stop();
								audio[1].Stop();
								audio[2].Stop();
								if (skierModel.frozenTimer == 0) {

									audio[2].pitch = UnityEngine.Random.Range(0.8f, 1.2f);
									audio[2].Play();
//									audio[0].clip = painClips[painIndex];
//									audio[0].Play();
//									AudioSource.PlayClipAtPoint(painClips[painIndex], skierView.transform.position);
								}else {
									if (NetworkCenter.Instance.GetPlayerNumber() == i){
										audio[2].PlayOneShot(failClip);
									}
								}

								int painIndex = UnityEngine.Random.Range(0,9);
								audio[1].volume = 1;
								audio[1].pitch = UnityEngine.Random.Range(0.8f, 1.2f);
								audio[1].PlayOneShot(painClips[painIndex]);

								skierCollisionSoundActivated[i] = true;
							}

							if (skierModel.fallenTimer == 0 && skierModel.frozenTimer > 20 && UnityEngine.Random.Range(0, 100) < 5){
								int painIndex = UnityEngine.Random.Range(0,9);
								audio[1].volume = 0.5f;
								audio[1].pitch = UnityEngine.Random.Range(0.8f, 1.2f);
								audio[1].PlayOneShot(painClips[painIndex]);
							}

						}
					}

				}

			}
		}
	}


	private void UpdatePosition(SkierModel skierModel, GameObject obj, Vector3 targetPos){
		float dist = Vector3.Distance(obj.transform.position, targetPos);
		if (skierModel.frozenTimer == 0){
			obj.transform.position = Vector3.Lerp(obj.transform.position, targetPos, lerpTimeFactor);
		}else {
			obj.transform.position = Vector3.Lerp(obj.transform.position, targetPos, lerpFrozenTimeFactor);
		}
	}
	
	

	public void SwitchPlayers(int oldPlayerId, int newPlayerId = 0){
		// game object
		GameObject tmpView = skierViews[oldPlayerId];
		skierViews[oldPlayerId] = skierViews[newPlayerId];
		skierViews[newPlayerId] = tmpView;
		// leaderboard
		string tmpName = skiersNames[oldPlayerId];
		skiersNames[oldPlayerId] = skiersNames[newPlayerId];
		skiersNames[newPlayerId] = tmpName;
		int tmpFinished = finishedSkiers[oldPlayerId];
		finishedSkiers[oldPlayerId] = finishedSkiers[newPlayerId];
		finishedSkiers[newPlayerId] = tmpFinished;
	}

	
	
	#region Views Creation
	

	private GameObject CreateSkier(bool own){
		
		GameObject mainObj = GameObject.Instantiate(skierPrefab, new Vector3(0,0,10), Quaternion.identity) as GameObject;

		if (own) {
			mainObj.AddComponent<CameraTracker> ();
		}

//		Color color;
//		if (own){
//			color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
//		}else {
//			color = new Color(UnityEngine.Random.Range(0.2f, 1.0f), UnityEngine.Random.Range(0.2f, 1.0f), UnityEngine.Random.Range(0.25f, 1.0f), 1.0f);
//		}
//		SkinnedMeshRenderer[] renderers = mainObj.GetComponentsInChildren<SkinnedMeshRenderer>();
//		foreach(SkinnedMeshRenderer renderer in renderers) {
//			renderer.material.color = color;
//		}

		return mainObj;
	}


	private GameObject CreateView(float width, float height, Color color){
		//New mesh and game object
		GameObject obj = new GameObject();
		obj.name = "plane";
		Mesh mesh = new Mesh();
		
		//Components
		MeshFilter meshFilter= obj.AddComponent<MeshFilter>();
		MeshRenderer meshRenderer= obj.AddComponent<MeshRenderer>();
		
		//Create a square mesh
		Vector3[] vertexes = new Vector3[4];
		vertexes[0] = new Vector3(0, 0);
		vertexes[1] = new Vector3(0,  height);
		vertexes[2] = new Vector3(width,  height);
		vertexes[3] = new Vector3(width, 0);
		mesh = CreateMesh(vertexes, color);
		
		//Assign materials
		meshRenderer.sharedMaterial = meshesMaterial;
		
		//Assign mesh to game object
		meshFilter.mesh = mesh;

		return obj;
	}
	
	
	private Mesh CreateMesh(Vector3[] vertexes, Color color){

		int i; // Counter used in for cycles
		
		// Create a new mesh
		Mesh mesh = new Mesh();

		// UVs
		Vector2[] uvs= new Vector2[vertexes.Length];
		
		for(i = 0; i < vertexes.Length; ++i){
			uvs[i] = new Vector2(0,0);
		}
		
		// Create triangles
		int[] tris= new int[3 * (vertexes.Length - 2)];    //3 verts per triangle * num triangles
		int C1;
		int C2;
		int C3;

		C1 = 0;
		C2 = 1;
		C3 = 2;
		
		for(i = 0 ; i < tris.Length ; i += 3){
			tris[i] = C1;
			tris[i+1] = C2;
			tris[i+2] = C3;
			
			C2++;
			C3++;
		}

		// Setup vertexes colors (here using all with the same color)
		Color[] colors = new Color[vertexes.Length];
		for (i = 0 ; i < vertexes.Length ; ++i){
			colors[i] = color;
		}
		
		//Assign data to mesh
		mesh.vertices = vertexes;
		mesh.uv = uvs;
		mesh.triangles = tris;
		mesh.colors = colors;
		
		//Recalculations
		mesh.RecalculateNormals();
		mesh.RecalculateBounds();  
		mesh.Optimize();
		
		//Name the mesh
		mesh.name = "mesh";
		
		//Return the mesh
		return mesh;
	}


	private void normalizeAnglesDifference(ref float a1, float a2){
		while (a1 - a2 > 180){
			a1 -= 360;
		}
		while (a1 - a2 < -180){
			a1 += 360;
		}
	}

#endregion


	public override void OnDestroy(WorldModel model){
		if (skierViews == null) return;
		foreach (GameObject skierView in skierViews) {
			if (skierView != null){
				GameObject.Destroy(skierView);
			}
		}

		GameObject leaderboard = GuiMenus.Instance.leaderboardObject;
		leaderboard.SetActive(false);
	}


	
}

