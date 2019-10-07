# message-manager
list, move and delete service bus messages from queues and topics

## list messages on dead letter queue with details
sbmgr list -g <resource group name> --namespace-name <servicebus namespace name>  -t Queue --keyName <name of shared access policy> --queue-name <name of queue> --dead  --details

## list messages on dead letter queue with details
sbmgr list -g <resource group name> --namespace-name <servicebus namespace name>  -t Queue --keyName <name of shared access policy> --queue-name <name of queue> --dead  --details

## delete a message (move it to deadletter queue)
sbmgr delete -g  <resource group name> --namespace-name <servicebus namespace name>  -t Queue --keyName <name of shared access policy> --queue-name <name of queue> --id <messageId of message>

### add --dead to the delete to remove from deadletter
permanently deletes the message
sbmgr delete -g  <resource group name> --namespace-name <servicebus namespace name>  -t Queue --keyName <name of shared access policy> --queue-name <name of queue> --id <messageId of message> --dead

## move a message to deadletter queue
sbmgr kill -g  <resource group name> --namespace-name <servicebus namespace name>  -t Queue --keyName <name of shared access policy> --queue-name <name of queue> --id <messageId of message>
