﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IdlessChaye.IdleToolkit.AVGEngine {
    public sealed class PachiGrimoire : MonoBehaviour {
        #region Singleton
        private static PachiGrimoire instance;
        public static PachiGrimoire I {
            get {
                if (instance == null) {
                    instance = FindObjectOfType(typeof(PachiGrimoire)) as PachiGrimoire;
                    if (instance == null) {
                        instance = ((GameObject)Instantiate(Resources.Load<GameObject>("AVGEngine/PachiGrimoire"))).GetComponent<PachiGrimoire>();
                    }
                }
                return instance;
            }
        }

        public void Awake() {
            DontDestroyOnLoad(gameObject);
            if (instance == null) {
                instance = this;
            } else {
                Destroy(gameObject);
            }

            Initialize();
        }
        #endregion
        public bool isShutDown;
        public bool isDebugMode;

        public ConstData constData;

        public UnityEngine.UI.Text text;

        public StateMachineManager StateMachine => stateMachine;

        public FileManager FileManager => fileManager;
        public ConfigManager ConfigManager => configManager;

        public UITexture t;

        private StateMachineManager stateMachine = new StateMachineManager(VoidState.Instance);

        private FileManager fileManager;

        private ConfigManager configManager = new ConfigManager();

        private PlayerRecordManager playerRecordManager = new PlayerRecordManager();

        private ResourceManager resourceManager = new ResourceManager();

        IEnumerator TestGetBgImage() {
            yield return new WaitForSeconds(1f);
            for(int i = 0; i < 100; i ++) {
                string name = i.ToString();
                name = "BI_" + name;
                Texture2D tex = resourceManager.Get<Texture2D>(name);
                t.mainTexture = tex;
                yield return new WaitForSeconds(1f);
            }
        }

        public void Initialize() {
            if(isShutDown)
                return;
            fileManager = new FileManager(configManager, playerRecordManager, resourceManager, constData);
            /*configManager.Config.PlayerIdentifier = "0xFFFFFFFF";
            configManager.SaveConfigContext();*/
            //playerRecordManager.PlayerRecord.markList.Add("呜呜呜");
            //playerRecordManager.SavePlayerRecord();
            StateMachine.TransferStateTo(InitState.Instance);
            //StartCoroutine(TestGetBgImage());

            if (isDebugMode) {
                text.text = configManager.Config.PlayerIdentifier;
                text.text = playerRecordManager.PlayerRecord.markList[0];
            }
        }


    }
}