namespace JRD_Projects.Models
{ 
    public class VisitorLog
    {
        public int Id { get; set; }
        public string? VisitorEmail { get; set; }
        public DateTime? Timestamp { get; set; }
        public string? Location { get; set; }
       
        // New tracking columns
        public bool ClickAIDocuChat { get; set; }        
        public bool ClickAngular { get; set; }
        public bool ClickDeliverySim { get; set; }
        public bool ClickReact { get; set; }
        public bool ClickElevator { get; set; }
        public bool ClickSubway { get; set; }
        public bool ClickPython { get; set; }
        public bool ClickJSPuzzle { get; set; }
        public bool ClickBattleships { get; set; }        
        public bool ClickBCATPAI { get; set; }
    } 
}
