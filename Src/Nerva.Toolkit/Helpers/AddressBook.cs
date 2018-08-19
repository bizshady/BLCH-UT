using System;
using System.Collections.Generic;
using System.IO;
using AngryWasp.Helpers;
using AngryWasp.Logger;
using AngryWasp.Serializer;

namespace Nerva.Toolkit.Helpers
{
    public class AddressBookEntry
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public string Address { get; set; }

        public string PaymentId { get; set; }
    }
     
    public class AddressBook
    {
        [SerializerInclude]
        private List<AddressBookEntry> entries;

        public List<AddressBookEntry> Entries => entries;

        private static AddressBook instance;
        //TODO: Add this to the settings so the path can be changed
        private static readonly string file = Path.Combine(Environment.CurrentDirectory, "AddressBook.xml");
        public static AddressBook Instance => instance;

        public static AddressBook New()
        {
            return new AddressBook
            {
                entries = new List<AddressBookEntry>()
            };
        }

        public static void Load()
        {
            if (!File.Exists(file))
            {
                instance = New();
        
                Log.Instance.Write("Address Book created at '{0}'", file);
                new ObjectSerializer().Serialize(instance, file);
            }
            else
            {
                Log.Instance.Write("Address Book loaded from '{0}'", file);
                instance = new ObjectSerializer().Deserialize<AddressBook>(XHelper.LoadDocument(file));
            }
        }

        public static void Save()
        {
            new ObjectSerializer().Serialize(instance, file);
            Log.Instance.Write("Address Book saved to '{0}'", file);
        }
    }
}