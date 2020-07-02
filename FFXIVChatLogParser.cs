using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FFXIVAPP.IPluginInterface;
using FFXIVAPP.IPluginInterface.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Test.Bot.FFXIV
{
    class FakePluginHost : FFXIVAPP.IPluginInterface.IPluginHost
    {
        event EventHandler<ActionContainersEvent> IPluginHost.ActionContainersUpdated
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        public event EventHandler<ChatLogItemEvent> ChatLogItemReceived;
        public event EventHandler<ConstantsEntityEvent> ConstantsUpdated;
        public event EventHandler<CurrentPlayerEvent> CurrentPlayerUpdated;

        event EventHandler<InventoryContainersEvent> IPluginHost.InventoryContainersUpdated
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event EventHandler<ActorItemsAddedEvent> IPluginHost.MonsterItemsAdded
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event EventHandler<ActorItemsRemovedEvent> IPluginHost.MonsterItemsRemoved
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event EventHandler<ActorItemsEvent> IPluginHost.MonsterItemsUpdated
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event EventHandler<NetworkPacketEvent> IPluginHost.NetworkPacketReceived
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event EventHandler<ActorItemsAddedEvent> IPluginHost.NPCItemsAdded
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event EventHandler<ActorItemsRemovedEvent> IPluginHost.NPCItemsRemoved
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event EventHandler<ActorItemsEvent> IPluginHost.NPCItemsUpdated
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event EventHandler<PartyMembersAddedEvent> IPluginHost.PartyMembersAdded
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event EventHandler<PartyMembersRemovedEvent> IPluginHost.PartyMembersRemoved
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event EventHandler<PartyMembersEvent> IPluginHost.PartyMembersUpdated
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event EventHandler<ActorItemsAddedEvent> IPluginHost.PCItemsAdded
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event EventHandler<ActorItemsRemovedEvent> IPluginHost.PCItemsRemoved
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event EventHandler<ActorItemsEvent> IPluginHost.PCItemsUpdated
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        event EventHandler<TargetInfoEvent> IPluginHost.TargetInfoUpdated
        {
            add
            {
                throw new NotImplementedException();
            }

            remove
            {
                throw new NotImplementedException();
            }
        }

        void IPluginHost.PopupMessage(string pluginName, FFXIVAPP.Common.Models.PopupContent content)
        {
            throw new NotImplementedException();
        }

        public void SendChatlogMessage(byte[] message)
        {
            ChatLogItemReceived.Invoke(this, new ChatLogItemEvent(this, new Sharlayan.Core.ChatLogItem()
            {
                Bytes = message,
                Code = "",
                Line = ""
            }));
        }

        public void UpdateWorldName(string worldName)
        {
            ConstantsUpdated.Invoke(this, new ConstantsEntityEvent(this, new FFXIVAPP.Common.Core.Constant.ConstantsEntity
            {
                ServerName = worldName
            }));
        }

        internal void InitConsts()
        {
            UpdateWorldName("Cerberus");
            CurrentPlayerUpdated.Invoke(this, new CurrentPlayerEvent(this, new Sharlayan.Core.CurrentPlayer
            {
                Name = "Dis Cord"
            }));
        }
    }

    public class FakeDiscordHandler : FFXIVAPP.Plugin.Log.ChatHandler.IDiscord
    {
        public void Broadcast(string message)
        {
            brodcastMessages.Push(message);
        }

        public void SetIsActive(bool active)
        {
            throw new NotImplementedException();
        }

        private Stack<string> brodcastMessages = new Stack<string>(1);
        public byte[] RetrieveLastBroadcastAsUTF8()
        {
            return System.Text.Encoding.UTF8.GetBytes(brodcastMessages.Pop());
        }
    }

    [TestClass]
    public class ChatLogParserTest
    {
        private static FakePluginHost fakePluginHost;
        private static FakeDiscordHandler fakeDiscordHandler;
        private static FFXIVAPP.Plugin.Log.ChatHandler.FFXIV FFXIVHandler;

        [ClassInitialize]
        public static void Setup(TestContext context)
        {
            fakePluginHost = new FakePluginHost();
            fakeDiscordHandler = new FakeDiscordHandler();
            FFXIVHandler = new FFXIVAPP.Plugin.Log.ChatHandler.FFXIV(fakePluginHost);
            fakePluginHost.InitConsts();
            FFXIVHandler.MainAsync(fakeDiscordHandler).Wait();
        }

        [TestInitialize]
        public void ResetTestEnvironment()
        {
            fakePluginHost.UpdateWorldName("Cerberus");
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
        /// A specfic set of byte streams generated by FFXIV will be converted to a human readable representation
        /// </summary>
        [TestMethod]
        [DynamicData(nameof(GetChatLogEntries), DynamicDataSourceType.Method)]
        public void UserReadableOutput(string fileName, byte[] rawData, byte[] resultData)
        {
            fakePluginHost.SendChatlogMessage(rawData);
            CollectionAssert.AreEqual(resultData, fakeDiscordHandler.RetrieveLastBroadcastAsUTF8());
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
            string newWorldName = "clearlydifferent";

            var rawData = ExtractResource("FFXIVAPP.Plugin.Discord.Test.TestData.FFXIVChat.localuser.binary");
            var resultData = System.Text.Encoding.UTF8.GetBytes(
                System.Text.Encoding.UTF8.GetString(
                    ExtractResource("FFXIVAPP.Plugin.Discord.Test.TestData.FFXIVChat.localuser.result")
                ).Replace("@Cerberus", "@"+ newWorldName)
            );

            fakePluginHost.UpdateWorldName(newWorldName);

            fakePluginHost.SendChatlogMessage(rawData);

            CollectionAssert.AreEqual(resultData, fakeDiscordHandler.RetrieveLastBroadcastAsUTF8());
        }
    }
}
