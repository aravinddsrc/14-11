using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel;

namespace DSRCManagementSystem.Models
{
    public class LDHomeModel
    {
        public List<UpcomingTraningModel> upcomingTrainings;
        public List<NominatedTrainingModel> nominatedTrainings;
        public List<HistorytrainingModel> historyTrainings;
        public List<Conductedtrainingmodel> conductedtrainings;
        public List<HistorytrainingModel> unattendedTrainings;
        public int TrainingId { get; set; }
        public string TrainingName { get; set; }
        public string TechnologyName { get; set; }
            [DisplayName("Schedule Date")]
        public DateTime? ScheduledDate { get; set; }
        public string Instructor { get; set; }
    }

    public class UpcomingTraningModel
    {
        public int TrainingId { get; set; }
        public string TrainingName { get; set; }
        public string TechnologyName { get; set; }
            [DisplayName("Schedule Date")]
        public DateTime? ScheduledDate { get; set; }
        public string Instructor { get; set; }
        public int? instructorid { get; set; }
          [DisplayName("Start Time")]
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public  int? Seatingcapacity { get;set;}
        public  Boolean IsSeatingCapacity { get; set; }

        //public string Endtime { get; set; }
        public int? Nominations { get; set; }
        public int submit { get; set; }
        public int pending { get; set; }


    }

    public class NominatedTrainingModel
    {

        public int? NominationId { get; set; }
        public int? TrainingId { get; set; }
        public string TrainingName { get; set; }
        public string TechnologyName { get; set; }
            [DisplayName("Schedule Date")]
        public DateTime? ScheduledDate { get; set; }
        public string Instructor { get; set; }
        [DisplayName("Start Time")]
        public string Starttime { get; set; }
        public Boolean test { get; set; }
        public Boolean? IsCompleted { get; set; }
    }

    public class HistorytrainingModel
    {
        public int TrainingId { get; set; }
        public string TrainingName { get; set; }
        public string TechnologyName { get; set; }
        [DisplayName("Schedule Date")]
        public DateTime? ScheduledDate { get; set; }
        public string Instructor { get; set; }
        public int FeedbackCount { get; set; }
        public bool? flag { get; set; }
    }

    public class Conductedtrainingmodel
    {

        public int? NominationId { get; set; }
        public int? TrainingId { get; set; }
        public string TrainingName { get; set; }
        public string TechnologyName { get; set; }
        [DisplayName("Schedule Date")]
        public DateTime? ScheduledDate { get; set; }
        public string Instructor { get; set; }
        [DisplayName("Start Time")]
        public string Starttime { get; set; }

       // public string Technology { get; set; }
        public string Endtime { get; set; }
        public int? Nominations { get; set; }
        public int submit { get; set; }
        public int pending { get; set; }
        public bool? IsCompleted { get; set; }



      //  public object TrainingID { get; set; }
    }



}