using EasyRegistry;
using System.Collections;

namespace ConsoleApp1 {
    class Program {
        static void Main(string[] args) {
            //instance for text
            var shiBa = new Dog {
                color = color.golden,
                Name = "Shiba",
                Size = 7,
                gender = "female",
                master = new master { name = "chen" },
                masters = new master[] {
                new master { name = "zhang" },
                new master { name = "wang" },
                new master { name = "li" },
                },
                text = new int[] {
                    1,2,3,4,5,6,7,8,9,10,11,12
                }
            };
            
            var siberianHusky = new Dog {
                color = color.black,
                Name = "¹þÊ¿Ææ",
                Size = 7,
                gender = "male",
                master = new master { name = "ling" }
            };
            var d1 = new Dog[] { shiBa,siberianHusky };

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
        public string gender;
        public master master;
        public master[] masters;
        public int[] text;
        
        public string introduction() {
            return $"Name£º{Name} Size£º{Size} gender£º{gender} color£º{color} master£º{master.name}";
        }
    }
}