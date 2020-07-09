namespace FFXIVAPP.Plugin.Discord.Test
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using FFXIVAPP.IPluginInterface;
    using FFXIVAPP.IPluginInterface.Events;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    class FakePluginHost : IPluginHost
    {
        event EventHandler<ActionContainersEvent> IPluginHost.ActionContainersUpdated
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        public event EventHandler<ChatLogItemEvent> ChatLogItemReceived;
        public event EventHandler<ConstantsEntityEvent> ConstantsUpdated;
        public event EventHandler<CurrentPlayerEvent> CurrentPlayerUpdated;

        event EventHandler<InventoryContainersEvent> IPluginHost.InventoryContainersUpdated
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        event EventHandler<ActorItemsAddedEvent> IPluginHost.MonsterItemsAdded
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        event EventHandler<ActorItemsRemovedEvent> IPluginHost.MonsterItemsRemoved
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        event EventHandler<ActorItemsEvent> IPluginHost.MonsterItemsUpdated
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        event EventHandler<NetworkPacketEvent> IPluginHost.NetworkPacketReceived
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        event EventHandler<ActorItemsAddedEvent> IPluginHost.NPCItemsAdded
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        event EventHandler<ActorItemsRemovedEvent> IPluginHost.NPCItemsRemoved
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        event EventHandler<ActorItemsEvent> IPluginHost.NPCItemsUpdated
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        event EventHandler<PartyMembersAddedEvent> IPluginHost.PartyMembersAdded
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        event EventHandler<PartyMembersRemovedEvent> IPluginHost.PartyMembersRemoved
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        event EventHandler<PartyMembersEvent> IPluginHost.PartyMembersUpdated
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        event EventHandler<ActorItemsAddedEvent> IPluginHost.PCItemsAdded
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        event EventHandler<ActorItemsRemovedEvent> IPluginHost.PCItemsRemoved
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        event EventHandler<ActorItemsEvent> IPluginHost.PCItemsUpdated
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        event EventHandler<TargetInfoEvent> IPluginHost.TargetInfoUpdated
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        void IPluginHost.PopupMessage(string pluginName, Common.Models.PopupContent content)
        {
            throw new NotImplementedException();
        }

        public void SendChatlogMessage(byte[] message)
        {
            if (this.ChatLogItemReceived == null)
            {
                throw new NotImplementedException("Handler for receiving chat log items needs to be set up in order to properly execute tests");
            }
            this.ChatLogItemReceived.Invoke(this, new ChatLogItemEvent(this, new Sharlayan.Core.ChatLogItem()
            {
                Bytes = message,
                Code = "",
                Line = ""
            }));
        }

        public void UpdateWorldName(string worldName)
        {
            if (this.ConstantsUpdated == null)
            {
                throw new NotImplementedException("Handler for constants updates needs to be set up in order to properly execute tests");
            }
            this.ConstantsUpdated.Invoke(this, new ConstantsEntityEvent(this, new Common.Core.Constant.ConstantsEntity
            {
                ServerName = worldName
            }));
        }

        internal void InitConstants()
        {
            this.UpdateWorldName("Cerberus");

            if (this.CurrentPlayerUpdated == null)
            {
                throw new NotImplementedException("Handler for player updates needs to be set up in order to properly execute tests");
            }
            this.CurrentPlayerUpdated.Invoke(this, new CurrentPlayerEvent(this, new Sharlayan.Core.CurrentPlayer
            {
                Name = "Dis Cord"
            }));
        }
    }

    public class FakeDiscordHandler : ChatHandler.IDiscord
    {
#pragma warning disable 1998
        public async Task Broadcast(string message)
        {
            this._broadcastMessages.Push(message);
        }
#pragma warning restore 1998

        public void SetIsActive(bool active)
        {
            throw new NotImplementedException();
        }

        private readonly Stack<string> _broadcastMessages = new Stack<string>(1);
        public byte[] RetrieveLastBroadcastAsUTF8()
        {
            return System.Text.Encoding.UTF8.GetBytes(this._broadcastMessages.Pop());
        }
    }

    [TestClass]
    public class ChatLogParserTest
    {
        private static FakePluginHost _fakePluginHost;
        private static FakeDiscordHandler _fakeDiscordHandler;
        private static ChatHandler.FFXIV _ffxivHandler;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            if (context == null) {
                throw new ArgumentNullException(nameof(context));
            }

            _fakePluginHost = new FakePluginHost();
            _fakeDiscordHandler = new FakeDiscordHandler();
            _ffxivHandler = new ChatHandler.FFXIV(_fakePluginHost);
            _fakePluginHost.InitConstants();
            _ffxivHandler.MainAsync(_fakeDiscordHandler).Wait();
        }

        [TestInitialize]
        public void ResetTestEnvironment()
        {
            _fakePluginHost.UpdateWorldName("Cerberus");
        }

        private static byte[] ExtractResource(string filename)
        {
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            using (Stream resFilestream = a.GetManifestResourceStream(filename))
            {
                if (resFilestream == null)
                {
                    return null;
                }

                byte[] ba = new byte[resFilestream.Length];
                resFilestream.Read(ba, 0, ba.Length);
                return ba;
            }
        }

        /// <summary>
        /// A specific set of byte streams generated by FFXIV will be converted to a human readable representation
        /// </summary>
        [TestMethod]
        [DynamicData(nameof(GetChatLogEntries), DynamicDataSourceType.Method)]
        public void UserReadableOutput(string fileName, byte[] rawData, byte[] resultData)
        {
            _fakePluginHost.SendChatlogMessage(rawData);
            CollectionAssert.AreEqual(resultData, _fakeDiscordHandler.RetrieveLastBroadcastAsUTF8());
        }

        public static IEnumerable<object[]> GetChatLogEntries()
        {
            var currentAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            return currentAssembly.GetManifestResourceNames()
                    .Where(resourcePath => resourcePath.EndsWith(".binary"))
                    .Where(resourcePath => resourcePath.Contains("FFXIVChat")).Select(pathToBinary => new object[] {
                        pathToBinary.Replace(".binary", ""),
                        ExtractResource(pathToBinary),
                        ExtractResource(pathToBinary.Replace(".binary", ".result"))
                    });
        }

        /// <summary>
        /// Append the name of the world the bot is connected to to messages sent from characters on that same world
        /// </summary>
        [TestMethod]
        public void AppendBotWorldName()
        {
            string newWorldName = "clearly_different";

            var rawData = ExtractResource("FFXIVAPP.Plugin.Discord.Test.TestData.FFXIVChat.localuser.binary");
            var resultData = System.Text.Encoding.UTF8.GetBytes(
                System.Text.Encoding.UTF8.GetString(
                    ExtractResource("FFXIVAPP.Plugin.Discord.Test.TestData.FFXIVChat.localuser.result")
                ).Replace("@Cerberus", "@"+ newWorldName)
            );

            _fakePluginHost.UpdateWorldName(newWorldName);

            _fakePluginHost.SendChatlogMessage(rawData);

            CollectionAssert.AreEqual(resultData, _fakeDiscordHandler.RetrieveLastBroadcastAsUTF8());
        }
    }
}
