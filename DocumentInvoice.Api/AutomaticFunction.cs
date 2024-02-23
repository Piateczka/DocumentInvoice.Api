using Azure.Storage.Queues.Models;
using DocumentInvoice.Service.Command;
using MediatR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DocumentInvoice.Api
{
    public class AutomaticFunction
    {
        private readonly ILogger<AutomaticFunction> _logger;
        private readonly IMediator _mediator;


        public AutomaticFunction(ILogger<AutomaticFunction> logger, IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [Function(nameof(DocumentProcess))]
        public async Task DocumentProcess([QueueTrigger("documenttoprocess", Connection = "TriggerConnectionString")] QueueMessage message)
        {
            await _mediator.Send(new PersistDocumentInfoCommand
            {
                Message = message.Body.ToString()
            });
        }

        [Function(nameof(DocumentAnalysis))]
        public async Task DocumentAnalysis([QueueTrigger("documenttoanalysis", Connection = "TriggerConnectionString")] QueueMessage message)
        {

            var request = JsonConvert.DeserializeObject<AnalysisDocumentCommand>(message.Body.ToString());
            await _mediator.Send(request);
        }
}
}
