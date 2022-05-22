using EasyRegistry;
using System.Collections;

namespace ConsoleApp1 {
    class Program {
        static void Main(string[] args) {
            //instance for text
            Dog shiBa = new Dog {
                color = color.golden,
                Name = "²ñÈ®",
                Size = 7,
                sex = "¹«",
                master = new master { name = "³ÂÃþÓã" },
                masters = new master[] {
                new master { name = "ÕÅÃþÓã" },
                new master { name = "ÍõÃþÓã" },
                new master { name = "ÀîÃþÓã" },
                },
                text = new int[] {
                    1,2,3,4,5,6,7,8,9,10,11,12
                }
            };
            Dog shiBa1 = shiBa;
            Dog shiBa2 = shiBa;
            Dog siberianHusky = new Dog {
                color = color.black,
                Name = "¹þÊ¿Ææ",
                Size = 7,
                sex = "¹«",
                master = new master { name = "³ÂÃþÓã" }
            };
            Dog[] d1 = new Dog[] { shiBa, shiBa1, shiBa2, siberianHusky };

            //Serialize text
            RegistryConvert.Serialize(@"SOFTWARE", d1);
            Console.WriteLine("Complete the injection");
            
            //Deserialize text
            var a = RegistryConvert.Deserialize<Dog[]>(@"SOFTWARE\ConsoleApp1.Dog[]");
            foreach (var item in a) {
                Console.WriteLine(item.introduction());
            }
        }
    }
    public class master {
        public string name;
    }
    public enum color {
        white,
        golden,
        black,
    }
    public class Dog
    {
        public int Size { get; set; }
        public string Name { get; set; }
        public color color;
        public string sex;
        public master master;
        public master[] masters;
        public int[] text;
        public string introduction() {
            return $"Name£º{Name} Size£º{Size} sex£º{sex} yanse£º{color} master£º{master.name}";
        }
    }
}