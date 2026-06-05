/// <reference lib="webworker" />
import { SocketClient } from '../types/SocketClient';

let socketClient: SocketClient | null = null;

addEventListener('message', ({ data }) => {

  if (!socketClient) {
    socketClient = new SocketClient();
  }
  switch (data.type) {

    case 'init':
      socketClient ??= new SocketClient();
      socketClient.connect('ws://localhost:5194/ws');

      break;
    case 'send':
      socketClient.send(data.chunk);
      break;
    case 'stop':
      socketClient.stop();
      break;
    case 'close':
      //socketClient?.close();
      //socketClient = null;
      break;
  }

});
