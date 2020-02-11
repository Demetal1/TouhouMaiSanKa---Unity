﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IdlessChaye.IdleToolkit.AVGEngine {
    public class MusicBGMStopNode : FunNode {
        public override void Interpret(ScriptSentenceContext context) {
            context.SkipToken("MusicStop");
            InterpretPart(context);
        }

        protected override void OnUpdateEngineState() {

        }

        protected override void OnUpdateStageContext() {
            if (paraList.Count != 0)
                throw new System.Exception("EngineScriptLoadFileNode");
            PachiGrimoire.I.MusicManager.BGMStop();
        }

        protected override void OnUpdateStageRender() {

        }

        protected override void OnLateUpdate() {

        }

    }
}