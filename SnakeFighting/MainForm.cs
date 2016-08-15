using NetFwTypeLib;
using SnakeFighting.GameMode;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SnakeFighting
{
    public partial class MainForm : Form
    {
        private Game game = null;

        public MainForm()
        {
            InitializeComponent();
            
            CheckFirewall();

            Size = new Size(800, 600);

            game = new SoloGame(this);
            game.Started += Game_Started;
            game.Exited += Game_Exited;
            Task.Run(() => game.Run());
        }

        private void Game_Started(object sender, EventArgs e)
        {
        }

        private void Game_Exited(object sender, EventArgs e)
        {
        }

        private void CheckFirewall()
        {
#if !DEBUG
#warning UAC Manifest requireAdministrator
#endif
            Type fwp = Type.GetTypeFromProgID("HNetCfg.FwPolicy2");
            INetFwPolicy2 firewallPolicy = Activator.CreateInstance(fwp) as INetFwPolicy2;
            if (firewallPolicy == null)
                throw new NotSupportedException("Create HNetCfg.FwPolicy2 failed");
            INetFwRule firewallRule = null;
            try
            {
                firewallRule = firewallPolicy.Rules.Item(Application.ProductName);
            }
            catch(Exception)
            {
                Type fwr = Type.GetTypeFromProgID("HNetCfg.FWRule");
                firewallRule = Activator.CreateInstance(fwr) as INetFwRule;
                if (firewallRule == null)
                    throw new NotSupportedException("Create HNetCfg.FWRule failed");
                firewallRule.Name = Application.ProductName;
                firewallRule.Action = NET_FW_ACTION_.NET_FW_ACTION_ALLOW;
                firewallRule.ApplicationName = Application.ExecutablePath;
                firewallRule.Profiles = (int)NET_FW_PROFILE_TYPE2_.NET_FW_PROFILE2_ALL;
                firewallRule.Protocol = (int)NET_FW_IP_PROTOCOL_.NET_FW_IP_PROTOCOL_ANY;
                firewallRule.Direction = NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN;
                firewallRule.Enabled = true;
                firewallRule.InterfaceTypes = "All";

                firewallPolicy.Rules.Add(firewallRule);
            }

            //Type fwmgr = Type.GetTypeFromProgID("HNetCfg.FwMgr");
            //INetFwMgr netFwMgr = Activator.CreateInstance(fwmgr) as INetFwMgr;
            //INetFwAuthorizedApplications aapps = netFwMgr.LocalPolicy.CurrentProfile.AuthorizedApplications;
            //Type fwaa = Type.GetTypeFromProgID("HNetCfg.FwAuthorizedApplication");
            //INetFwAuthorizedApplication aapp = null;
            //try
            //{
            //    aapp = aapps.Item(Application.ExecutablePath);
            //}
            //catch (Exception)
            //{
            //    aapp = Activator.CreateInstance(fwaa) as INetFwAuthorizedApplication;
            //    aapp.Enabled = true;
            //    aapp.IpVersion = NET_FW_IP_VERSION_.NET_FW_IP_VERSION_ANY;
            //    aapp.Name = Application.ProductName;
            //    aapp.ProcessImageFileName = Application.ExecutablePath;
            //    aapp.Scope = NET_FW_SCOPE_.NET_FW_SCOPE_ALL;
            //    aapps.Add(aapp);
            //}
        }
    }
}
