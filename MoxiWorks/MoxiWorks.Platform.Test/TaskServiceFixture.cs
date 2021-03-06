﻿using NUnit.Framework;
using static NUnit.Framework.Assert;

namespace MoxiWorks.Platform.Test
{
    [TestFixture]
    public class TaskServiceFixture
    {
        [Test]
        public void ShouldReturnATask()
        {
            var taskJson = StubDataLoader.LoadJsonFile("Task.json");  
           
            var service = new TaskService(new MoxiWorksClient(new StubContextClient(taskJson)));
            var response = service.GetTaskAsync("foo", AgentIdType.AgentUuid, "1234", "12345").Result; 
            IsInstanceOf<Task>(response.Item);
        }

        [Test]
        public void ShouldHanldeApiReturnedErrors()
        {
            var json = StubDataLoader.LoadJsonFile("FailureResponse.json"); 
            var service = new TaskService(new MoxiWorksClient(new StubContextClient(json)));
            var response = service.GetTaskAsync("foo", AgentIdType.AgentUuid, "1234", "12345").Result;
            Assert.IsTrue((bool?) response.HasErrors); 
            
        }

       
    }


}
