﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Telephony;
using Xamarin.Contacts;
using Xamarin.Forms;
using XF.Contatos.Droid.AppResources;
using XF.Contatos.Global;
using XF.Contatos.Models;

[assembly: Dependency(typeof(ContatoHelper))]
namespace XF.Contatos.Droid.AppResources
{
    public class ContatoHelper : IContatoHelper
    {
        public async Task<bool> GetContatoListAsync()
        {
            var context = MainApplication.CurrentContext as Activity;
            if (context == null) return false;

            var idRequestCode = 0;

            var contactsPermission = Manifest.Permission.ReadContacts;
			var phonePermission = Manifest.Permission.CallPhone;
			string[] permissions = { contactsPermission, phonePermission };
            
			if (context.CheckSelfPermission(phonePermission) != (int)Permission.Granted)
            {
                context.RequestPermissions(permissions, idRequestCode);
            }

            if (context.CheckSelfPermission(contactsPermission) != (int) Permission.Granted)
            {
                context.RequestPermissions(permissions, idRequestCode);
            }

            var book = new AddressBook(context);
            if (!await book.RequestPermission())
            {
                Console.WriteLine("Permissão negada pelo usuário!");
                return false;
            }

            publishList(book.ToList());
            return true;
        }

        public bool LigarParaContato(Contato contato)
        {
            var context = MainApplication.CurrentContext as Activity;
            if (context == null) return false;

            var intent = new Intent(Intent.ActionCall);
            intent.SetData(Android.Net.Uri.Parse("tel:" + contato.Numero));

            if (IsIntentAvailable(context, intent))
            {
                context.StartActivity(intent);
                return true;
            }

            return false;
        }

        public static bool IsIntentAvailable(Context context, Intent intent)
        {
            var packageManager = context.PackageManager;

            var list = packageManager.QueryIntentServices(intent, 0)
                .Union(packageManager.QueryIntentActivities(intent, 0));

            if (list.Any()) return true;

            var manager = TelephonyManager.FromContext(context);
            return manager.PhoneType != Android.Telephony.PhoneType.None;
        }

        private void publishList(List<Contact> contactList)
        {
            var contatos = new List<Contato>();

            foreach (var cont in contactList)
            {
                contatos.Add(new Contato
                {
                    Nome = cont.DisplayName,
                    Numero = cont.Phones.FirstOrDefault()?.Number,
                    //Thumbnail = cont.GetThumbnail().ToArray<byte>()
                });
            }

            MessagingCenter.Send<IContatoHelper, List<Contato>>(this, "obtercontatos", contatos);
        }
    }
}