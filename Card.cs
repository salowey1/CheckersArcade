using System.Collections.Generic;

namespace SovietReigns
{
    public class Card
    {
        public string Text { get; set; }
        public string LeftChoice { get; set; }
        public string RightChoice { get; set; }

        // Ключи: "ideology", "economy", "military", "trust"
        public Dictionary<string, int> LeftEffects { get; set; } = new();
        public Dictionary<string, int> RightEffects { get; set; } = new();
    }
}