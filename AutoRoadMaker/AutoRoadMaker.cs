using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LitJson;
using UnityEngine;
using UnityEngine.Networking;

namespace MaxIceFlameTemplate.Basic
{
    public class AutoRoadMaker : MonoBehaviour
    {
        public string ChartPath;
        public float timer = 0;//计时器
        private MainLine MainLineCom;
        public bool gameStarted;
        private float[] TimeArray;//note时间存储器
        private int currentNote = 0;
        public void StartRoadMaking() { gameStarted = true; }
        // Start is called before the first frame update
        void Start()
        {
            MainLineCom = GetComponent<MainLine>();
            if (File.Exists(ChartPath))
            {
                //读取并转为JSON数据
                JsonData ChartData = JsonMapper.ToObject(File.ReadAllText(ChartPath));
                int NoteCount = ChartData["note_list"].Count; //计算note数量
                Debug.Log("总Note数量：" + NoteCount);
                TimeArray = new float[NoteCount]; //各note秒数存储
                int TempoCount = ChartData["tempo_list"].Count; //计算变速数量
                float BPM = 60000000 / (int)ChartData["tempo_list"][0]["value"]; //计算初始BPM
                Debug.Log("初始BPM：" + BPM);
                int TempoStepUp = 0, nextTempoTick = 0, lastTempoTick = 0;
                float currentTime = 0f;
                if (TempoCount > 1)
                {
                    TempoStepUp = 1; //变速计数器
                    nextTempoTick = (int)ChartData["tempo_list"][TempoStepUp]["tick"];
                    Debug.Log("下一处变速的tick：" + nextTempoTick);
                }
                else
                {
                    Debug.Log("未检测到变速。");
                }
                for (int i = 0; i < NoteCount; i++)
                {
                    int j = i + 1;
                    int currentTick = (int)ChartData["note_list"][i]["tick"]; //读取note的tick
                    if (currentTick == nextTempoTick)
                    { //判断是否需要变速
                        BPM = 60000000 / (int)ChartData["tempo_list"][TempoStepUp]["value"]; //计算新的BPM
                        Debug.Log("已变速！当前BPM：" + BPM);
                        lastTempoTick = nextTempoTick;
                        currentTime = TimeArray[i - 1] + (0.000001f * (int)ChartData["tempo_list"][TempoStepUp - 1]["value"]);
                        if (TempoStepUp < TempoCount - 1)
                        {
                            TempoStepUp++; //计数器递增
                            nextTempoTick = (int)ChartData["tempo_list"][TempoStepUp]["tick"];
                            Debug.Log("下一处变速的tick：" + nextTempoTick);
                        }
                    }
                    float currentBeat = (currentTick - lastTempoTick) / 480f, currentSPB = 60f / BPM; //分别计算当前音符节拍数、每拍用时
                    Debug.Log("第" + j + "个音符的Tick：" + currentTick);
                    //Debug.Log ("第" + j + "个音符的节拍数：" + currentBeat);
                    //Debug.Log ("当前每拍用时：" + currentSPB);
                    TimeArray[i] = currentBeat * currentSPB + currentTime; //存储note秒数
                    Debug.Log("第" + j + "个音符的时间：" + TimeArray[i] + "秒");
                }
            }
            else
            {
                Debug.LogError("位于" + ChartPath + "的文件不存在！");
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (gameStarted) timer += Time.deltaTime;//从线开始动时启动计时器
            if ((timer - Time.deltaTime) <= TimeArray[currentNote] && (timer + Time.deltaTime) >= TimeArray[currentNote]){//若当前时间前后各1帧内有目标时间
                GameObject cube = this.GetComponent<RoadMaker>().cube; 
                float roadWidth = this.GetComponent<RoadMaker>().roadWidth; 
                this.GetComponent<RoadMaker>().MakeRoad(cube, roadWidth, MainLineCom);//铺路
                MainLineCom.ChangeDirection();//转向
                Debug.Log("已完成第" + (currentNote+1) + "处转向！");
                currentNote++;//准备下一个转向点
            }
        }
    }
}
