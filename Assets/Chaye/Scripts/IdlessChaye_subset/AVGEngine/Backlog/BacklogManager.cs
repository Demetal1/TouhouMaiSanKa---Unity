﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IdlessChaye.IdleToolkit.AVGEngine {
    public class BacklogManager :IRecordable{
        public List<BacklogItem> BacklogItemList { get; private set; }
        public int Capacity => capacity;
        public int Count => count;
        public int Head => head;

        private int capacity;
        private int count;
        private int head;

        public BacklogManager(int capacity) {
            BacklogItemList = new List<BacklogItem>(capacity);
            this.capacity = capacity;
            this.count = 0;
            this.head = -1;
        }

        public BacklogItem Seek(int offsetIndex) {
            int index = SeekIndex(offsetIndex);
            return BacklogItemList[index];
        }

        public void Push(string voiceIndex, string contextIndex, string imageIndex, string name) {
            BacklogItem item = new BacklogItem(voiceIndex, contextIndex, imageIndex, name);
            Push(item);
        }
        public void Push(BacklogItem item) {
            int index = (head + 1) % capacity;
            if (count < capacity) {
                if (count + 1 != index) {
                    throw new System.Exception("BacklogManager Push");
                }
                count++;
                BacklogItemList.Add(item);
            } else {
                BacklogItemList[index] = item;
            }
            head = index;
        }

        private int SeekIndex(int offsetIndex) {
            int index = head - offsetIndex;
            while (index < 0)
                index += count;
            return index % count;
        }

        public void LoadPlayerData() {
            throw new System.NotImplementedException();
        }

        public void LoadStoryData() {
            throw new System.NotImplementedException();
        }

        //private void Foreach() {
        //    BacklogManager backlogManager = PachiGrimoire.I.BacklogManager;
        //    for (int i = 0; i < backlogManager.Count; i++) {
        //        BacklogItem item = backlogManager.Seek(i);
        //    }
        //}
    }
}