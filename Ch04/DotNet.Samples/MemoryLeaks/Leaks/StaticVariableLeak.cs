using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RandomNameGeneratorLibrary;

namespace MemoryLeaks.Leaks
{
    class StaticVariableLeak : IMemoryLeakExample
    {
        private static List<Person> persons = new List<Person>();

        public void Run()
        {
            ulong index = 0;
            var ageGenerator = new Random();
            var personGenerator = new PersonNameGenerator();
            while (index < 10_000_000_000_000)
            {
                string firstname = personGenerator.GenerateRandomFirstName();
                string lastname = personGenerator.GenerateRandomLastName();
                uint age = (uint) ageGenerator.Next(1, 100);
                persons.Add(new Person(firstname, lastname, age));
                //Console.WriteLine($"Generated person {persons.Count}: {firstname} {lastname} ({age})");
                index++;
                if (index % 10_000 == 0)
                {
                    Console.WriteLine($"Created {persons.Count}"); 
                }
            }
            Console.WriteLine($"Created {persons.Count}");
        }
    }

    class Person
    {
        private string firstname;
        private string lastname;
        private uint age;
        private DateTime dateOfBirth;

        public Person(string firstname, string lastname, uint age)
        {
            this.Firstname = firstname;
            this.Lastname = lastname;
            this.Age = age;
        }

        public string Firstname
        {
            get { return firstname; }
            set { firstname = value; }
        }

        public string Lastname
        {
            get { return lastname; }
            set { lastname = value; }
        }

        public uint Age
        {
            get { return age; }
            set { age = value; }
        }
    }
}
