using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DemoDataBuilder.OutputModel;

namespace DemoDataBuilder
{
    public static class Rand
    {
        private static readonly Random Random = new Random();

        public static bool Boolean(double probabilityTrue = 0.5)
        {
            return Random.NextDouble() < probabilityTrue;
        }

        public static int Int(int minValue, int maxValue)
        {
            return Random.Next(minValue, maxValue);
        }

        public static DateTime Date(int startYear)
        {
            DateTime startDate = new DateTime(startYear, 1, 1);

            int maxDays = (int) (DateTime.Today - startDate).TotalDays;

            return startDate.AddDays(Random.Next(maxDays));
        }

        public static DateTime Date(DateTime minDate, DateTime maxDate)
        {
            int maxDays = (int)(maxDate - minDate).TotalDays;

            return minDate.AddDays(Random.Next(maxDays));
        }

        public static DateTime WeekDayDate(DateTime minDate, DateTime maxDate)
        {
            int maxDays = (int)(maxDate - minDate).TotalDays;

            DateTime result;

            do
            {
                result = minDate.AddDays(Random.Next(maxDays));
            } while (result.DayOfWeek == DayOfWeek.Saturday || result.DayOfWeek == DayOfWeek.Sunday);

            return result;
        }

        public static string String()
        {
            int length = Random.Next(10, 32);

            StringBuilder stringBuilder = new StringBuilder();

            for (int i = 0; i < length; i++)
            {
                stringBuilder.Append((char)Random.Next(32, 126));
            }

            return stringBuilder.ToString();
        }

        #region Phone Numbers

        public static string Phone()
        {
            return string.Format("({0:000}) {1:000}-{2:0000}", Random.Next(200, 1000), Random.Next(200, 1000), Random.Next(0, 10000));
        }

        public static string Phone(IEnumerable<int> areaCodes)
        {
            if (areaCodes == null)
                return Phone();

            List<KeyValuePair<int, double>> codes =
                areaCodes.Select(code => new KeyValuePair<int, double>(code, 1.0d)).ToList();

            return Phone(codes);
        }

        public static string Phone(ICollection<KeyValuePair<int, double>> areaCodes)
        {
            if (areaCodes == null || areaCodes.Count == 0)
                return Phone();

            int areaCode = 800;

            double position = Random.NextDouble() * areaCodes.Sum(x => x.Value);

            foreach (KeyValuePair<int, double> keyValuePair in areaCodes)
            {
                areaCode = keyValuePair.Key;

                if (position < keyValuePair.Value)
                    break;

                position -= keyValuePair.Value;
            }

            return string.Format("({0:000}) {1:000}-{2:0000}", areaCode, Random.Next(200, 1000), Random.Next(0, 10000));
        }

        public static int WeightedInt(params double[] weights)
        {
            if (weights == null || weights.Length == 0)
                throw new ArgumentNullException("weights");

            double position = Random.NextDouble()*weights.Sum();

            for (int i = 0; i < weights.Length; i++)
            {
                if (position < weights[i])
                    return i;

                position -= weights[i];
            }

            return weights.Length;
        }

        #endregion

        #region Names and Titles

        public static string Title()
        {
            switch (Random.Next(0, 10))
            {
                case 1: return "Mr.";
                case 2: return "Mrs.";
                case 3: return "Miss.";
                case 4: return "Dr.";
                case 5: return "Sir";
                default: return null;
            }
        }

        public static string LastName()
        {
            string[] names = {
                                 "Smith", "Johnson", "Williams", "Brown", "Jones", "Miller", "Davis", "García", "Rodríguez",
                                 "Wilson", "Martínez", "Anderson", "Taylor", "Thomas", "Hernández", "Moore", "Martin",
                                 "Jackson", "Thompson", "White", "López", "Lee", "González", "Harris", "Clark", "Lewis",
                                 "Robinson", "Walker", "Pérez", "Hall", "Young", "Allen", "Sánchez", "Wright", "King",
                                 "Scott", "Green", "Baker", "Adams", "Nelson", "Hill", "Ramírez", "Campbell", "Mitchell",
                                 "Roberts", "Carter", "Phillips", "Evans", "Turner", "Torres", "Parker", "Collins",
                                 "Edwards", "Stewart", "Flores", "Morris", "Nguyen", "Murphy", "Rivera", "Cook", "Rogers",
                                 "Morgan", "Peterson", "Cooper", "Reed", "Bailey", "Bell", "Gómez", "Kelly", "Howard",
                                 "Ward", "Cox", "Díaz", "Richardson", "Wood", "Watson", "Brooks", "Bennett", "Gray",
                                 "James", "Reyes", "Cruz", "Hughes", "Price", "Myers", "Long", "Foster", "Sanders", "Ross",
                                 "Morales", "Powell", "Sullivan", "Russell", "Ortiz", "Jenkins", "Gutiérrez", "Perry",
                                 "Butler", "Barnes", "Fisher"
                             };

            return names[Random.Next(0, names.Length)];
        }

        public static string FemaleFirstName()
        {
            string[] names = {
                                 "Emma", "Isabella", "Emily", "Madison", "Ava", "Olivia", "Sophia", "Abigail",
                                 "Elizabeth", "Chloe", "Samantha", "Addison", "Natalie", "Mia", "Alexis", "Alyssa",
                                 "Hannah", "Ashley", "Ella", "Sarah", "Grace", "Taylor", "Brianna", "Lily", "Hailey",
                                 "Anna", "Victoria", "Kayla", "Lillian", "Lauren", "Kaylee", "Allison", "Savannah",
                                 "Nevaeh", "Gabriella", "Sofia", "Makayla", "Avery", "Riley", "Julia", "Leah", "Aubrey",
                                 "Jasmine", "Audrey", "Katherine", "Morgan", "Brooklyn", "Destiny", "Sydney", "Alexa",
                                 "Kylie", "Brooke", "Kaitlyn", "Evelyn", "Layla", "Madeline", "Kimberly", "Zoe",
                                 "Jessica", "Peyton", "Alexandra", "Claire", "Madelyn", "Maria", "Mackenzie", "Arianna",
                                 "Jocelyn", "Amelia", "Angelina", "Trinity", "Andrea", "Maya", "Valeria", "Sophie",
                                 "Rachel", "Vanessa", "Aaliyah", "Mariah", "Gabrielle", "Katelyn", "Ariana", "Bailey",
                                 "Camila", "Jennifer", "Melanie", "Gianna", "Charlotte", "Paige", "Autumn", "Payton",
                                 "Faith", "Sara", "Isabelle", "Caroline", "Genesis", "Isabel", "Mary", "Zoey", "Gracie",
                                 "Megan", "Haley", "Mya", "Michelle", "Molly", "Stephanie", "Nicole", "Jenna", "Natalia"
                                 , "Sadie", "Jada", "Serenity", "Lucy", "Ruby", "Eva", "Kennedy", "Rylee", "Jayla",
                                 "Naomi", "Rebecca", "Lydia", "Daniela", "Bella", "Keira", "Adriana", "Lilly", "Hayden",
                                 "Miley", "Katie", "Jade", "Jordan", "Gabriela", "Amy", "Angela", "Melissa", "Valerie",
                                 "Giselle", "Diana", "Amanda", "Kate", "Laila", "Reagan", "Jordyn", "Kylee", "Danielle",
                                 "Briana", "Marley", "Leslie", "Kendall", "Catherine", "Liliana", "Mckenzie",
                                 "Jacqueline", "Ashlyn", "Reese", "Marissa", "London", "Juliana", "Shelby", "Cheyenne",
                                 "Angel", "Daisy", "Makenzie", "Miranda", "Erin", "Amber", "Alana", "Ellie", "Breanna",
                                 "Ana", "Mikayla", "Summer", "Piper", "Adrianna", "Jillian", "Sierra", "Jayden",
                                 "Sienna", "Alicia", "Lila", "Margaret", "Alivia", "Brooklynn", "Karen", "Violet",
                                 "Sabrina", "Stella", "Aniyah", "Annabelle", "Alexandria", "Kathryn", "Skylar", "Aliyah"
                                 , "Delilah", "Julianna", "Kelsey", "Khloe", "Carly", "Amaya", "Mariana", "Christina",
                                 "Alondra", "Tessa", "Eliana", "Bianca", "Jazmin", "Clara", "Vivian", "Josephine",
                                 "Delaney", "Scarlett", "Elena", "Cadence", "Alexia", "Maggie", "Laura", "Nora", "Ariel"
                                 , "Elise", "Nadia", "Mckenna", "Chelsea", "Lyla", "Alaina", "Jasmin", "Hope", "Leila",
                                 "Caitlyn", "Cassidy", "Makenna", "Allie", "Izabella", "Eden", "Callie", "Haylee",
                                 "Caitlin", "Kendra", "Karina", "Kyra", "Kayleigh", "Addyson", "Kiara", "Jazmine",
                                 "Karla", "Camryn", "Alina", "Lola", "Kyla", "Kelly", "Fatima", "Tiffany"
                             };

            return names[Random.Next(0, names.Length)];
        }

        public static string MaleFirstName()
        {
            string[] names = {
                                 "Jacob", "Michael", "Ethan", "Joshua", "Daniel", "Alexander", "Anthony", "William",
                                 "Christopher", "Matthew", "Jayden", "Andrew", "Joseph", "David", "Noah", "Aiden",
                                 "James", "Ryan", "Logan", "John", "Nathan", "Elijah", "Christian", "Gabriel",
                                 "Benjamin", "Jonathan", "Tyler", "Samuel", "Nicholas", "Gavin", "Dylan", "Jackson",
                                 "Brandon", "Caleb", "Mason", "Angel", "Isaac", "Evan", "Jack", "Kevin", "Jose",
                                 "Isaiah", "Luke", "Landon", "Justin", "Lucas", "Zachary", "Jordan", "Robert", "Aaron",
                                 "Brayden", "Thomas", "Cameron", "Hunter", "Austin", "Adrian", "Connor", "Owen", "Aidan"
                                 , "Jason", "Julian", "Wyatt", "Charles", "Luis", "Carter", "Juan", "Chase", "Diego",
                                 "Jeremiah", "Brody", "Xavier", "Adam", "Carlos", "Sebastian", "Liam", "Hayden",
                                 "Nathaniel", "Henry", "Jesus", "Ian", "Tristan", "Bryan", "Sean", "Cole", "Alex",
                                 "Eric", "Brian", "Jaden", "Carson", "Blake", "Ayden", "Cooper", "Dominic", "Brady",
                                 "Caden", "Josiah", "Kyle", "Colton", "Kaden", "Eli", "Miguel", "Antonio", "Parker",
                                 "Steven", "Alejandro", "Riley", "Richard", "Timothy", "Devin", "Jesse", "Victor",
                                 "Jake", "Joel", "Colin", "Kaleb", "Bryce", "Levi", "Oliver", "Oscar", "Vincent",
                                 "Ashton", "Cody", "Micah", "Preston", "Marcus", "Max", "Patrick", "Seth", "Jeremy",
                                 "Peyton", "Nolan", "Ivan", "Damian", "Maxwell", "Alan", "Kenneth", "Jonah", "Jorge",
                                 "Mark", "Giovanni", "Eduardo", "Grant", "Collin", "Gage", "Omar", "Emmanuel", "Trevor",
                                 "Edward", "Ricardo", "Cristian", "Nicolas", "Kayden", "George", "Jaxon", "Paul",
                                 "Braden", "Elias", "Andres", "Derek", "Garrett", "Tanner", "Malachi", "Conner",
                                 "Fernando", "Cesar", "Javier", "Miles", "Jaiden", "Alexis", "Leonardo", "Santiago",
                                 "Francisco", "Cayden", "Shane", "Edwin", "Hudson", "Travis", "Bryson", "Erick", "Jace",
                                 "Hector", "Josue", "Peter", "Jaylen", "Mario", "Manuel", "Abraham", "Grayson", "Damien"
                                 , "Kaiden", "Spencer", "Stephen", "Edgar", "Wesley", "Shawn", "Trenton", "Jared",
                                 "Jeffrey", "Landen", "Johnathan", "Bradley", "Braxton", "Ryder", "Camden", "Roman",
                                 "Asher", "Brendan", "Maddox", "Sergio", "Israel", "Andy", "Lincoln", "Erik", "Donovan",
                                 "Raymond", "Avery", "Rylan", "Dalton", "Harrison", "Andre", "Martin", "Keegan", "Marco"
                                 , "Jude", "Sawyer", "Dakota", "Leo", "Calvin", "Kai", "Drake", "Troy", "Zion",
                                 "Clayton", "Roberto", "Zane", "Gregory", "Tucker", "Rafael", "Kingston", "Dominick",
                                 "Ezekiel", "Griffin", "Devon", "Drew", "Lukas", "Johnny", "Ty", "Pedro", "Tyson",
                                 "Caiden"
                             };

            return names[Random.Next(0, names.Length)];
        }

        public static string FirstName()
        {
            return FirstName(null);
        }

        public static string FirstName(string title)
        {
            switch (title)
            {
                case "Mr.":
                case "Sir":
                    return MaleFirstName();

                case "Mrs.":
                case "Miss.":
                    return FemaleFirstName();

                default:
                    return Random.NextDouble() < 0.52 ? MaleFirstName() : FemaleFirstName();
            }
        }

        public static string MiddleName()
        {
            double percentile = Random.NextDouble();

            if (percentile > 0.97)
                return string.Format("{0} {1}", FirstName(), FirstName());

            if (percentile > 0.8)
                return FirstName();

            return null;
        }

        #endregion

        #region words

        public static string Adjective()
        {
            string[] words = {
                                 "afraid", "agreeable", "amused", "ancient", "angry", "annoyed", "anxious", "arrogant",
                                 "ashamed", "average", "awful", "bad", "beautiful", "better", "big", "bitter", "black",
                                 "blue", "boiling", "brave", "breezy", "brief", "bright", "broad", "broken", "bumpy",
                                 "calm", "charming", "cheerful", "chilly", "clumsy", "cold", "colossal", "combative",
                                 "comfortable", "confused", "cooing", "cool", "cooperative", "courageous", "crazy",
                                 "creepy", "cruel", "cuddly", "curly", "curved", "damp", "dangerous", "deafening",
                                 "deep", "defeated", "defiant", "delicious", "delightful", "depressed", "determined",
                                 "dirty", "disgusted", "disturbed", "dizzy", "dry", "dull", "dusty", "eager", "early",
                                 "elated", "embarrassed", "empty", "encouraging", "energetic", "enthusiastic", "envious"
                                 , "evil", "excited", "exuberant", "faint", "fair", "faithful", "fantastic", "fast",
                                 "fat", "few", "fierce", "filthy", "fine", "flaky", "flat", "fluffy", "foolish", "frail"
                                 , "frantic", "fresh", "friendly", "frightened", "funny", "fuzzy", "gentle", "giant",
                                 "gigantic", "good", "orgeous", "greasy", "great", "green", "grieving", "grubby",
                                 "grumpy", "handsome", "happy", "hard", "harsh", "healthy", "heavy", "helpful",
                                 "helpless", "high", "hilarious", "hissing", "hollow", "homeless", "horrible", "hot",
                                 "huge", "hungry", "hurt", "hushed", "husky", "icy", "ill", "immense", "itchy",
                                 "jealous", "jittery", "jolly", "juicy", "kind", "large", "late", "lazy", "light",
                                 "little", "lively", "lonely", "long", "loose", "loud", "lovely", "low", "lucky",
                                 "magnificent", "mammoth", "many", "massive", "melodic", "melted", "mighty", "miniature"
                                 , "moaning", "modern", "mute", "mysterious", "narrow", "nasty", "naughty", "nervous",
                                 "new", "nice", "nosy", "numerous", "nutty", "obedient", "obnoxious", "odd", "old",
                                 "orange", "ordinary", "outrageous", "panicky", "perfect", "petite", "plastic",
                                 "pleasant", "precious", "pretty", "prickly", "proud", "puny", "purple", "purring",
                                 "quaint", "quick", "quickest", "quiet", "rainy", "rapid", "rare", "raspy", "ratty",
                                 "red", "relieved", "repulsive", "resonant", "ripe", "roasted", "robust", "rotten",
                                 "rough", "round", "sad", "salty", "scary", "scattered", "scrawny", "screeching",
                                 "selfish", "shaggy", "shaky", "shallow", "sharp", "shivering", "short", "shrill",
                                 "silent", "silky", "silly", "skinny", "slimy", "slippery", "slow", "small", "smiling",
                                 "smooth", "soft", "solid", "sore", "sour", "spicy", "splendid", "spotty", "square",
                                 "squealing", "stale", "steady", "steep", "sticky", "stingy", "straight", "strange",
                                 "striped", "strong", "successful", "sweet", "swift", "tall", "tame", "tan", "tart",
                                 "tasteless", "tasty", "tender", "tender", "tense", "terrible", "testy", "thirsty",
                                 "thoughtful", "thoughtless", "thundering", "tight", "tiny", "tired", "tough", "tricky",
                                 "troubled", "ugliest", "ugly", "uneven", "upset", "uptight", "vast", "victorious",
                                 "vivacious", "voiceless", "wasteful", "watery", "weak", "weary", "wet", "whispering",
                                 "wicked", "wide", "wide-eyed", "witty", "wonderful", "wooden", "worried", "yellow",
                                 "young", "yummy", "zany"
                             };

            return words[Random.Next(0, words.Length)];
        }

        public static string Noun()
        {
            string[] words = {
                                 "ball", "bat", "bed", "book", "boy", "bun", "can", "cake", "cap", "car", "cat", "cow",
                                 "cub", "cup", "dad", "day", "dog", "doll", "dust", "fan", "feet", "girl", "gun", "hall"
                                 , "hat", "hen", "jar", "kite", "man", "map", "men", "mom", "pan", "pet", "pie", "pig",
                                 "pot", "rat", "son", "sun", "toe", "tub", "van", "apple", "arm", "banana", "bike",
                                 "bird", "book", "chin", "clam", "class", "clover", "club", "corn", "crayon", "crow",
                                 "crown", "crowd", "crib", "desk", "dime", "dirt", "dress", "fang", "field", "flag",
                                 "flower", "fog", "game", "heat", "hill", "home", "horn", "hose", "joke", "juice",
                                 "kite", "lake", "maid", "mask", "mice", "milk", "mint", "meal", "meat", "moon",
                                 "mother", "morning", "name", "nest", "nose", "pear", "pen", "pencil", "plant", "rain",
                                 "river", "road", "rock", "room", "rose", "seed", "shape", "shoe", "shop", "show",
                                 "sink", "snail", "snake", "snow", "soda", "sofa", "star", "step", "stew", "stove",
                                 "straw", "string", "summer", "swing", "table", "tank", "team", "tent", "test", "toes",
                                 "tree", "vest", "water", "wing", "winter", "woman", "women", "alarm", "animal", "aunt",
                                 "bait", "balloon", "bath", "bead", "beam", "bean", "bedroom", "boot", "bread", "brick",
                                 "brother", "camp", "chicken", "children", "crook", "deer", "dock", "doctor", "downtown"
                                 , "drum", "dust", "eye", "family", "father", "fight", "flesh", "food", "frog", "goose",
                                 "grade", "grandfather", "grandmother", "grape", "grass", "hook", "horse", "jail", "jam"
                                 , "kiss", "kitten", "light", "loaf", "lock", "lunch", "lunchroom", "meal", "mother",
                                 "notebook", "owl", "pail", "parent", "park", "plot", "rabbit", "rake", "robin", "sack",
                                 "sail", "scale", "sea", "sister", "soap", "song", "spark", "space", "spoon", "spot",
                                 "spy", "summer", "tiger", "toad", "town", "trail", "tramp", "tray", "trick", "trip",
                                 "uncle", "vase", "winter", "water", "week", "wheel", "wish", "wool", "yard", "zebra",
                                 "actor", "airplane", "airport", "army", "baseball", "beef", "birthday", "boy", "brush",
                                 "bushes", "butter", "cast", "cave", "cent", "cherries", "cherry", "cobweb", "coil",
                                 "cracker", "dinner", "eggnog", "elbow", "face", "fireman", "flavor", "gate", "glove",
                                 "glue", "goldfish", "goose", "grain", "hair", "haircut", "hobbies", "holiday", "hot",
                                 "jellyfish", "ladybug", "mailbox", "number", "oatmeal", "pail", "pancake", "pear",
                                 "pest", "popcorn", "queen", "quicksand", "quiet", "quilt", "rainstorm", "scarecrow",
                                 "scarf", "stream", "street", "sugar", "throne", "toothpaste", "twig", "volleyball",
                                 "wood", "wrench", "advice", "anger", "answer", "apple", "arithmetic", "badge", "basket"
                                 , "basketball", "battle", "beast", "beetle", "beggar", "brain", "branch", "bubble",
                                 "bucket", "cactus", "cannon", "cattle", "celery", "cellar", "cloth", "coach", "coast",
                                 "crate", "cream", "daughter", "donkey", "drug", "earthquake", "feast", "fifth",
                                 "finger", "flock", "frame", "furniture", "geese", "ghost", "giraffe", "governor",
                                 "honey", "hope", "hydrant", "icicle", "income", "island", "jeans", "judge", "", "ce",
                                 "lamp", "lettuce", "marble", "month", "north", "ocean", "patch", "plane", "playground",
                                 "poison", "riddle", "rifle", "scale", "seashore", "sheet", "sidewalk", "skate", "slave"
                                 , "sleet", "smoke", "stage", "station", "thrill", "throat", "throne", "title",
                                 "toothbrush", "turkey", "underwear", "vacation", "vegetable", "visitor", "voyage",
                                 "year", "able", "achieve", "acoustics", "action", "activity", "aftermath", "afternoon",
                                 "afterthought", "apparel", "appliance", "beginner", "believe", "bomb", "border",
                                 "boundary", "breakfast", "cabbage", "cable", "calculator", "calendar", "caption",
                                 "carpenter", "cemetery", "channel", "circle", "creator", "creature", "education",
                                 "faucet", "feather", "friction", "fruit", "fuel", "galley", "guide", "guitar", "health"
                                 , "heart", "idea", "kitten", "laborer", "language", "lawyer", "linen", "locket",
                                 "lumber", "magic", "minister", "mitten", "money", "mountain", "music", "partner",
                                 "passenger", "pickle", "picture", "plantation", "plastic", "pleasure", "pocket",
                                 "police", "pollution", "railway", "recess", "reward", "route", "scene", "scent",
                                 "squirrel", "stranger", "suit", "sweater", "temper", "territory", "texture", "thread",
                                 "treatment", "veil", "vein", "volcano", "wealth", "weather", "wilderness", "wren",
                                 "wrist", "writer"
                             };

            return words[Random.Next(0, words.Length)];
        }

        #endregion

        public static string PostalCode()
        {
            const string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            return string.Format("T{0}{1} {2}{3}{4}",
                                 Random.Next(0, 10),
                                 letters[Random.Next(0, 26)],
                                 Random.Next(0, 10),
                                 letters[Random.Next(0, 26)],
                                 Random.Next(0, 10));
        }

        public static string CalgaryStreetName()
        {
            string[] names =
                {
                    "1 Ave NE", "1 Ave NW", "1 St SW", "10 Ave SW", "10 St NW", "10A St NW", "10 St NW",
                    "11 Ave SW", "11 St SW", "112 Ave NW", "114 Ave SE", "118 Ave SE", "11 Ave SW", "12 Ave SW",
                    "12 St NE", "12 Ave SE", "137 Ave SE", "14 Ave NE", "14 Ave NW", "14 St NW", "14 St SW",
                    "146 Ave SE", "15 Ave NW", "16 Ave NE", "16 Ave NW", "16 Ave SW", "16 St SW", "17 Ave SE",
                    "17 Ave SW", "18 Ave NE", "18 St NE", "18 St SE", "19 Ave NE", "19 ST NE", "19 St NW", "19 St SW",
                    "2 Ave NW", "2 St SW", "20 Ave NW", "20 Ave SE", "20 St SW", "23 Ave NW", "24 Ave NE", "24 Ave SE",
                    "24 St SE", "24 St SW", "25 Ave SW", "25 St SW", "26 Ave NE", "26 Ave SE", "26 Ave SW", "27 Ave NE",
                    "27 St SW", "28 Ave SW", "28 St SE", "29 Ave NE", "29 Ave SW", "29 St NW", "3 Ave NW", "3 Ave SE",
                    "3 Ave SW", "3 St SW", "30 Ave SE", "30 St SW", "31 St SE", "32 Ave NE", "33 Ave NE", "33 Ave SW",
                    "33 St SW", "34 Ave SW", "35 St NE", "36 Ave NE", "36 St NE", "36 St SW", "36 St NE", "37 St SW",
                    "3 St SE", "3 St SW", "4 Ave SW", "4 St NE", "4 St NW", "4 St SW", "40 Ave NW", "41 Ave NE",
                    "42 Ave SE", "44 St NE", "45 Ave NE", "45th St SW", "48 Ave NW", "48 St SE", "49 Ave SW", "4 St NW",
                    "4 St SW", "50 St SE", "51 St SW", "52 Ave NW", "52 St NE", "52 St SE", "52 St NE", "53 Ave SW",
                    "54 Ave SW", "54 St SE", "54 Ave SW", "58 Ave SE", "6 Ave SW", "6 St NE", "6 St SW", "60 St NE",
                    "61 St SE", "62 Ave SE", "64 St NE", "67 Ave SW", "68 St NE", "68 St SE", "69 Ave SE", "69 St SW",
                    "7 Ave SW", "7 St SW", "71 Ave SW", "72 Ave NE", "77 Ave SE", "8 Ave SE", "8 Ave SW", "8 St NE",
                    "8 St SW", "85 St SW", "9 Ave SE", "9 Ave SW", "9 St SW", "90 Ave SW", "Acadia Dr SE",
                    "Arbour Close Glen NW", "Arbour Lake Dr NW", "Arbour Lake Way NW", "Arbour Stone Close NW",
                    "Aspen Glen Landing SW", "Aspen Glen Pl SW", "Aspen Glen Way SW", "Aspen Stone Blvd SW",
                    "Aspen Summit Dr SW", "Auburn Bay Hts SE", "Banff Trail NW", "Bannister Road SE",
                    "Barclay Parade SW", "Barlow Trail SE", "Beddington Blvd NW", "Bedford Dr NE", "Bonaventure Dr SE",
                    "Bonavista Dr SE", "Bow Bottom Trail SE", "Bow Trail SW", "Bowfort Rd NW", "Bowness Rd NW",
                    "Bowridge Dr NE", "Bowridge Dr NW", "Braeside Dr SW", "Brentwood Road NW", "Brightonwoods Gdns SE",
                    "Canada Olympic Rd NW", "Canyon Meadows Dr SE", "Canyon Meadows Dr SW", "Castleridge Blvd NE",
                    "Castleridge Close NE", "Castleridge Dr NE", "Centre St NE", "Centre St North", "Centre St NW",
                    "Centre St SE", "Centre St South", "Chritie Estate Blvd SW", "Citadel Close NW",
                    "Citadel Hills Circle NW", "Citadel Way NW", "Coach Bluff Cres SW", "Coach Hill Road SW",
                    "Coachway Rd SW", "Collegiate Blvd NW", "Copperfield Blvd SE", "Copperfield Common SE",
                    "Copperfield Terrace SE", "Copperleaf Way SE", "Copperstone Mews SE", "Copperstone St SE",
                    "Coral Shores Dr NE", "Coral Springs Blvd NE", "Coral Springs Circle", "Cougar Ridge Dr SW",
                    "Cougartown Close SW", "Country Hills Blvd NE", "Country Hills Blvd NW", "Country Hills Landing NW",
                    "Country Village Link NE", "Country Village Rd NE", "Country Village Road NE",
                    "Country Village Way NE", "Covehaven Rd NE", "Coventry Cres NE", "Coventry Dr NE",
                    "Coverdale Court NE", "Cresthaven Place SW", "Crowchild Trail NW", "Crowchild Trail SW",
                    "Crowfoot Circle NW", "Crowfoot Cres NW", "Crowfoot Road NW", "Crowfoot Terrace NW",
                    "Crowfoot Way NW", "Deer Park Pl SE", "Deer Ridge Dr SE", "Deerfield Terr SE", "Del Monica Bay NE",
                    "Discovery Ridge Crt SW", "Discovery Ridge Hill SW", "Discovery Ridge Terr SW",
                    "Douglas Glen Close SE", "Douglas Glen Grove SE", "Douglas Glen Point SE", "Douglas Woods Dr SE",
                    "Dovercliffe Rd SE", "Edenwold Dr NW", "Edgebrook Dr NW", "Edgedale Dr NW", "Edgeland Road NW",
                    "Edgemont Blvd NW", "Edgevalley Cir NW", "Edmonton Trail NE", "Elbow Dr SW", "Erin Meadow Way SE",
                    "Erin Pl SE", "Erin Woods Dr SE", "Evansmeade Circle NW", "Evansmeade Way NW",
                    "Evercreek Bluffs View SW", "Evercreek Bluffs Way SW", "Everglen Close SW", "Evergreen Sq SW",
                    "Everoak Close SW", "Everridge Dr SW", "Fairmount Dr SE", "Falconridge Blvd NE", "Falconridge Dr NE"
                    , "Falmere Way NE", "Falsbridge Dr NE", "Falsbridge Gate NE", "Falshire Dr NE", "Falshire Way NE",
                    "Flint Road SE", "Garrison Gate SW", "General Ave NE", "Glenmore Trail SW", "Greenview Dr NE",
                    "Grey Eagle Dr", "Hampstead Circle NW", "Hamptons Dr NW", "Hartford Rd NW", "Harvest Hills Blvd NE",
                    "Hawkfield Cres NW", "Hawksbrow Rd NW", "Hawkstone Dr NW", "Hawkwood Blvd NW", "Heritage Dr SW",
                    "Hidden Creek Dr NW", "Hidden Hills Way NW", "Hidden Valley Dr NW", "Hidden Valley Grove NW",
                    "High St SE", "James Mckevitt Road SW", "Kensington Rd NW", "La Caille Place SW",
                    "Lake Bonavista Dr SE", "Lake Fraser Dr SE", "Lake Sylvan Dr SE", "Lakeview Dr SW", "Laneham Pl SW",
                    "Larkspur Way SW", "Longridge Dr SW", "Macewan Dr NW", "Macewan Park Way NW", "Macleod Trail SE",
                    "Macleod Trail SW", "Manning Rd NE", "Maplecreek Dr SE", "Mapleglen Cres SE", "Marlborough Dr NE",
                    "Marthas Manor NE", "Martindale Blvd NE", "Martindale Dr NE", "Mayfair Pl", "Mcivor Blvd SE",
                    "Mckenzie Towne Ave SE", "Mcknight Blvd NE", "Mcleon Trail S W", "Mclvor Blvd SE", "Memorial Dr NE",
                    "Memorial Dr SE", "Meredith Rd NE", "Meridian Road NE", "Meridian Road SE", "Millrise Blvd SW",
                    "Mount Royal Gate SW", "Northland Dr NW", "Northmount Dr NW", "Oakfern Way SW", "Oakfield Dr SW",
                    "Ogden Rd SE", "Old Banff Coach Rd SW", "Olympic Way SE", "Palliser Dr SW", "Panamount Green NW",
                    "Panamount Heights NW", "Panamount Hill NW", "Panatella Blvd NW", "Panatella View NW",
                    "Panatellablvd NW", "Parkdale Blvd NW", "Parkdale Crescent NW", "Parkvalley Rd NW",
                    "Patterson Hill SW", "Pensville Close SE", "Pinecliff Dr NE", "Point Dr NW", "Point Mckay Cres NW",
                    "Portland St SE", "Prominence Pk SW", "Pumphouse Ave SW", "Queensland Dr SE", "Quesnay Wood Dr SW",
                    "Ranchlands Blvd NW", "Richard Road SW", "Richard Way SW", "Richmond Rd SW", "Richmond Road SW",
                    "Rideau Pl SW", "Riverglen Dr SE", "Rivervalley Dr SE", "Rockbluff Pl NW", "Rocky Ridge Bay NW",
                    "Rocky Ridge Dr NW", "Rockyledge View NW", "Rockyvalley Dr NW", "Royal Birch Cir NW",
                    "Royal Birch Rd NW", "Royal Oak Grove NW", "Rundlehorn Dr", "Saddlebrook NE", "Saddleridge Close NE"
                    , "Saddletowne Circle NE", "Sandarac Dr NW", "Sandstone Dr NW", "Sarcee Trail NW", "Scandia Hill NW"
                    , "Scenic Cove Court NW", "Scenic Glen Cres NW", "Shaganappi Trail NW", "Shawcliffe Gate SW",
                    "Shawfield Way SW", "Shawinigan Dr SW", "Shawmeadows Road SW", "Shawnee Way SW", "Shawville Blvd SE"
                    , "Shawville Blvd SW", "Sierra Morena Blvd SW", "Signal Hill Center SW", "Signal Hill Centre SW",
                    "Silver Brook Rd NW", "Silver Hill Way NW", "Silver Ridge Dr NW", "Silver Springs Blvd NW",
                    "Silver Springs Rd NW", "Silver Valley Bay NW", "Sirocco Dr SW", "Somercrest St SW",
                    "Somerset Pk SW", "Southland Dr SW", "Southport Rd SW", "Spruce Ctr SW", "Spruce Dr SW",
                    "Stewart Green SW", "Stradwick Way SW", "Strathclair Rise SW", "Strathcona Blvd SW",
                    "Strathcona Dr SW", "Strathcona Hill SW", "Symons Valley Rd NW", "Taradale Close NE",
                    "Taralake Cres NE", "Taralake Terrace NE", "Tarawood Lane NE", "Taringtion Landing NE",
                    "Temple Dr NE", "Templegreen Road NE", "Thornhill Place NW", "Tremblant Way SW", "Tuscany Blvd NW",
                    "Tuscany Hills Cir NW", "Tuscany Springs Blvd NW", "Tuscany Vista Cres NW", "Tuscarora Heights NW",
                    "University Dr NW", "Uxbridge Dr NW", "Val Gardena View SW", "Varsity Dr NW", "Wentworth Circle SW",
                    "Wentworth Pl", "West Winds Dr NE", "Weston Dr SW", "Westwinds Crescent NE", "Westwinds Dr NE",
                    "Whitefield Dr NE", "Whitehorn Dr NE", "Whitehorn Rd NE", "Willow Park Dr SE", "Woodpark Blvd SW",
                    "Woodview Dr SW", "Worcester Dr SW", "Zoo Road NE"
                };

            return names[Random.Next(0, names.Length)];
        }


        #region Entities

        public static Contact Contact()
        {
            Contact contact = new Contact();


            return contact;
        }

        #endregion

    }
}
