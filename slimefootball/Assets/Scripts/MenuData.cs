using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MenuData
{
    public static Common.GameMode GameMode { get; set; } = Common.GameMode.SinglePlayer1v1;
    public static Common.AiImplementations[] TeamAiImplementations { get; set; } = new Common.AiImplementations[] { Common.AiImplementations.Default, Common.AiImplementations.Default };
    public static int GameWidth { get; set; } = 32;
    public static int GameHeight { get; set; } = 20;
}
