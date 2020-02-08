﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace IdlessChaye.IdleToolkit.AVGEngine {
    public class ScriptManager {
        private ResourceManager resourceManager;
        private StageContextManager stageContextManager;

        private string scriptPointerScriptName { get { return stageContextManager.scriptPointerScriptName; } set { stageContextManager.scriptPointerScriptName = value; } }
        private int scriptPointerLineNumber { get { return stageContextManager.scriptPointerLineNumber; } set { stageContextManager.scriptPointerLineNumber = value; } }
        private List<string> scriptReplaceKeys { get { return stageContextManager.scriptReplaceKeys; } }
        private List<string> scriptReplaceValues { get { return stageContextManager.scriptReplaceValues; } }


        private Stack<char> charStack = new Stack<char>();
        private bool isSecondGear = false;

        private List<ScriptSentenceContext> scriptSentenceList;
        private Stack<string> pointerScriptNameStack = new Stack<string>();
        private Stack<int> pointerLineNumberStack = new Stack<int>();

        private List<ScriptSentenceContext> secondScriptSentenceList;
        private int secondScriptPointerLineNumber; // 第二档的脚本无名
        private bool isAllTrue;
        public bool IsAllTrue => isAllTrue;

        public ScriptManager(ResourceManager resourceManager, StageContextManager stageContextManager) {
            this.resourceManager = resourceManager;
            this.stageContextManager = stageContextManager;
        }



        public bool NextSentence() {
            if (isSecondGear)
                return NextSecondSentence();
            else
                return NextFirstSentence();
        }

        public void LoadScriptFile(string scriptName, string scriptContext) {
            isSecondGear = false;
            if (!IsOver()) {
                pointerScriptNameStack.Push(scriptPointerScriptName);
                pointerLineNumberStack.Push(scriptPointerLineNumber);
            }
            scriptSentenceList = ProcessScriptContext(scriptContext);
            scriptPointerScriptName = scriptName;
            scriptPointerLineNumber = 0;
        }

        public void LoadScriptContext(string scriptContext) {
            isSecondGear = true;
            secondScriptSentenceList = ProcessScriptContext(scriptContext);
            secondScriptPointerLineNumber = 0;
        }

        public void UnloadSecondSentence() {
            secondScriptSentenceList = null;
            secondScriptPointerLineNumber = -1;
            isSecondGear = false;
        }



        public void ScriptIfThenElse(string ifStr, string thenStr,string elseStr) {
            isAllTrue = true;
            LoadScriptContext(ifStr);
            while (isAllTrue && NextSentence()) ; // isAllTrue会在Execute中改变
            UnloadSecondSentence();
            if(isAllTrue) {
                LoadScriptContext(thenStr);
            } else {
                LoadScriptContext(elseStr);
            }
        }

        public void ScriptReplaceAdd(string key, string value) { // 后来居上
            stageContextManager.scriptReplaceKeys.Add(key);
            stageContextManager.scriptReplaceValues.Add(value);
        }




        private List<ScriptSentenceContext> ProcessScriptContext(string scriptContext) {
            if (scriptContext == null || scriptContext.Length == 0) {
                throw new System.Exception("传了个啥?");
            }

            List<ScriptSentenceContext> sentenceList = new List<ScriptSentenceContext>();

            // 1. "\r\n" -> "\n"
            scriptContext.Replace("\r\n", "\n");
            int lastIndex = scriptContext.Length - 1; // 给最后补上'\n'
            if (scriptContext[lastIndex] != '\n') {
                scriptContext = scriptContext + "\n";
            }

            // 2. <>和**的绝对存在
            charStack.Clear();
            List<string> tempList = new List<string>(); // 宅出去<>**剩下的字符串
            List<bool> tempIsCompleteList = new List<bool>(); // 标记<>**字符串
            int lastTailIndex = -1;
            int leftIndex = -1;
            int i;
            for (i = 0; i < scriptContext.Length; i++) {
                char ch = scriptContext[i];
                switch (ch) {
                    case '<':
                        if (charStack.Count == 0) {
                            charStack.Push('<');
                            leftIndex = i;
                            if (i != lastTailIndex + 1) {
                                string str = scriptContext.Substring(lastTailIndex + 1, i - lastTailIndex - 1);
                                tempList.Add(str);
                                tempIsCompleteList.Add(false);
                            }
                        } else if (charStack.Peek() == '<') {
                            charStack.Push('<');
                        } else if (charStack.Peek() == '*') {
                            // pass;
                        } else {
                            throw new System.Exception($"语法有问题,Stack在<前遇到了 {charStack.Peek()}");
                        }
                        break;
                    case '>':
                        if (charStack.Count == 0) {
                            throw new System.Exception("语法有问题,遇到了 >");
                        } else if (charStack.Peek() == '<') {
                            charStack.Pop();
                            if (charStack.Count == 0) { // <>收束
                                lastTailIndex = i;
                                string str = scriptContext.Substring(leftIndex + 1, i - leftIndex - 1);
                                tempList.Add(str);
                                tempIsCompleteList.Add(true);
                            }
                        } else if (charStack.Peek() == '*') {
                            // pass
                        } else {
                            throw new System.Exception($"语法有问题,Stack在>前遇到了 {charStack.Peek()}");
                        }
                        break;
                    case '*':
                        if (charStack.Count == 0) {
                            charStack.Push('*');
                            leftIndex = i;
                            if (i != lastTailIndex + 1) {
                                string str = scriptContext.Substring(lastTailIndex + 1, i - lastTailIndex - 1);
                                tempList.Add(str);
                                tempIsCompleteList.Add(false);
                            }
                        } else if (charStack.Peek() == '*') {
                            if (charStack.Count != 1) {
                                throw new System.Exception($"语法有问题,好多*");
                            }
                            // **收束
                            charStack.Pop();
                            lastTailIndex = i;
                            string str = scriptContext.Substring(leftIndex + 1, i - leftIndex - 1);
                            tempList.Add(str);
                            tempIsCompleteList.Add(true);
                        } else if (charStack.Peek() == '<') {
                            // pass
                        } else {
                            throw new System.Exception($"语法有问题,Stack在*前遇到了 {charStack.Peek()}");
                        }
                        break;
                    default:
                        break;
                }
            }
            if (charStack.Count != 0) {
                throw new System.Exception("Stack isn't clear");
            }
            int lastStrLength = i - lastTailIndex - 1; // 把最后一段字符串加上
            if (lastStrLength > 0) {
                tempList.Add(scriptContext.Substring(lastTailIndex + 1, lastStrLength));
                tempIsCompleteList.Add(false);
            }

            // 3. ScriptReplace宏替换
            for (i = 0; i < tempList.Count; i++) {
                bool isComplete = tempIsCompleteList[i];
                if (isComplete) {
                    continue;
                }
                // 宏替换
                string str = tempList[i];
                for (int j = scriptReplaceKeys.Count - 1; j >= 0; j--) {
                    string key = scriptReplaceKeys[j];
                    if (str.Contains(key)) {
                        string value = scriptReplaceValues[j];
                        str = str.Replace(key, value);
                    }
                }
                // 4. 空格消除
                str = str.Replace(" ", "");
                tempList[i] = str;
            }

            // 5. '\n'脚本割解
            // 6. 全分割,得到ScriptSentenceContext
            List<string> fragmentList = new List<string>();
            for (i = 0; i < tempList.Count; i++) {
                string str = tempList[i];
                bool isComplete = tempIsCompleteList[i];
                if (isComplete) {
                    fragmentList.Add(str);
                    continue;
                }
                // 处理str,并在其中找'\n'
                int j;
                lastTailIndex = -1;
                int length = -1;
                string smallStr;
                for (j = 0; j < str.Length; j++) {
                    char ch = str[j];
                    switch (ch) {
                        case '\n':
                            length = j - lastTailIndex - 1;
                            if (length != 0) {
                                throw new System.Exception("语法出错,\n前一定是),但事实不是!");
                            }
                            lastTailIndex = j;
                            ScriptSentenceContext scriptSentenceContext = new ScriptSentenceContext(fragmentList.ToArray());
                            if (scriptSentenceContext.IsCorrect == false) {
                                throw new System.Exception("ScriptSentence is not right!");
                            }
                            string currentToken = scriptSentenceContext.CurrentToken;
                            if (currentToken != null && !currentToken.Substring(0,2).Equals("//")) { 
                                sentenceList.Add(scriptSentenceContext);
                            }
                            fragmentList.Clear();
                            break;
                        case '_':
                            length = j - lastTailIndex - 1;
                            if (length > 0) {
                                smallStr = str.Substring(lastTailIndex + 1, length);
                                fragmentList.Add(smallStr);
                            }
                            lastTailIndex = j;
                            break;
                        case '(':
                            length = j - lastTailIndex - 1;
                            if (length > 0) {
                                smallStr = str.Substring(lastTailIndex + 1, length);
                                fragmentList.Add(smallStr);
                            }
                            fragmentList.Add("(");
                            lastTailIndex = j;
                            break;
                        case ')':
                            length = j - lastTailIndex - 1;
                            if (length > 0) {
                                smallStr = str.Substring(lastTailIndex + 1, length);
                                fragmentList.Add(smallStr);
                            }
                            fragmentList.Add(")");
                            lastTailIndex = j;
                            break;
                        case ',':
                            length = j - lastTailIndex - 1;
                            if (length > 0) {
                                smallStr = str.Substring(lastTailIndex + 1, length);
                                fragmentList.Add(smallStr);
                            }
                            lastTailIndex = j;
                            break;
                        default:
                            break;
                    }
                }

            }
            if (fragmentList.Count != 0) {
                foreach (string s in fragmentList)
                    Debug.LogWarning(s);
                throw new System.Exception("怎么还有剩余的?");
            }

            return sentenceList;
        }

        private bool IsOver() {
            if (scriptSentenceList == null)
                return true;
            int length = scriptSentenceList.Count;
            if (scriptPointerLineNumber < length)
                return false;
            else
                return true;
        }

        private bool IsSecondOver() {
            if (secondScriptSentenceList == null)
                return true;
            int length = secondScriptSentenceList.Count;
            if (secondScriptPointerLineNumber < length)
                return false;
            else
                return true;
        }

        private bool NextFirstSentence() {
            if (IsOver()) {
                if (pointerScriptNameStack.Count == 0) {
                    return false;
                } else {
                    // 恢复脚本上下文
                    scriptPointerScriptName = pointerScriptNameStack.Pop();
                    scriptPointerLineNumber = pointerLineNumberStack.Pop();
                    string scriptIndex = PachiGrimoire.I.constData.ScriptIndexPrefix + "_" + scriptPointerScriptName;
                    string scriptContext = PachiGrimoire.I.ResourceManager.Get<string>(scriptIndex);
                    scriptSentenceList = ProcessScriptContext(scriptContext);
                }
            }
            ScriptSentenceContext context = scriptSentenceList[scriptPointerLineNumber];
            scriptPointerLineNumber = scriptPointerLineNumber + 1;
            ExpressionRootNode rootNode = new ExpressionRootNode();
            rootNode.Interpret(context);
            rootNode.Execute();
            return true;
        }

        private bool NextSecondSentence() {
            if (IsSecondOver()) {
                return false;
            }
            ScriptSentenceContext context = secondScriptSentenceList[secondScriptPointerLineNumber];
            secondScriptPointerLineNumber = secondScriptPointerLineNumber + 1;
            ExpressionRootNode rootNode = new ExpressionRootNode();
            rootNode.Interpret(context);
            rootNode.Execute();
            return true;
        }
    }

}