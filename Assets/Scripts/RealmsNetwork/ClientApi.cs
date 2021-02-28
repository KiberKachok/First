using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RealmsNetwork;
using LiteNetLib.Utils;
using LiteNetLib;
using System.Linq;

namespace RealmsNetwork
{
    public partial class Client
    {
        public Palette palette;

        public NetworkedMonoBehaviour[] callbacksTargets;

        public void CreateRoom()
        {
            NetDataWriter roomCreateData = new NetDataWriter();
            roomCreateData.Put((byte)MessageType.CreateRoomRequest);
            server.Send(roomCreateData, DeliveryMethod.ReliableOrdered);
        }

        public void JoinRoom(string roomName)
        {
            NetDataWriter roomJoinData = new NetDataWriter();
            roomJoinData.Put((byte)MessageType.JoinRoomRequest);
            roomJoinData.Put(roomName);
            server.Send(roomJoinData, DeliveryMethod.ReliableOrdered);
        }

        public void LeaveRoom()
        {
            if (SceneManager.GetActiveScene().name != "Menu")
            {
                SceneManager.LoadScene("Menu");
            }
            NetDataWriter leaveRoomData = new NetDataWriter();
            leaveRoomData.Put((byte)MessageType.LeaveRoomRequest);
            server.Send(leaveRoomData, DeliveryMethod.ReliableOrdered);
        }

        public void StartGame()
        {
            NetDataWriter data = new NetDataWriter();
            data.Put((byte)MessageType.StartGameRequest);
            server.Send(data, DeliveryMethod.ReliableOrdered);
        }

        public void SetNickname(string nickname)
        {
            this.nickname = nickname;
            NetDataWriter changeNicknameData = new NetDataWriter();
            changeNicknameData.Put((byte)MessageType.ChangeNicknameRequest);
            changeNicknameData.Put(nickname);
            server.Send(changeNicknameData, DeliveryMethod.ReliableOrdered);
        }

        public void SendUnits(Region from, Region to, int kingdomId, int units)
        {
            NetDataWriter data = new NetDataWriter();
            data.Put((byte)MessageType.SendUnitsRequest);
            data.Put(from.id);
            data.Put(to.id);
            data.Put(Vector3.Distance(from.transform.position, to.transform.position));
            data.Put(kingdomId);
            data.Put(units);
            server.Send(data, DeliveryMethod.ReliableOrdered);
        }

        public void SendMessage(string ownHash, string targetHash, string message)
        {
            NetDataWriter data = new NetDataWriter();
            data.Put((byte)MessageType.SendMessageRequest);
            data.Put(ownHash);
            data.Put(targetHash);
            data.Put(message);
            server.Send(data, DeliveryMethod.ReliableOrdered);
        }

        public void OnConnectionStateUpdate(ConnectionState state)
        {
            foreach (var i in callbacksTargets) i.OnConnectionStateUpdate(state);
        }
    }
}
