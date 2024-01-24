using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.TwoFactorAuthenticator
{
    public class Settings : BaseModel
    {
        public Settings()
        {
        }
        
        public ObservableCollection<AuthenticatorItem> AuthenticatorItems { get; set; } = new();

    }


    public record AuthenticatorItem
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string KeyType { get; set; }
        public string Key { get; set; }
    }
}