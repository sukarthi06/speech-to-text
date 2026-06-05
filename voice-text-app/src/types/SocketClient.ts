import { AudioChunk } from "./audio-type";

export class SocketClient {

    private socket!: WebSocket;

    connect(url: string) {
        this.socket = new WebSocket(url);

        this.socket.onopen = () => {
            console.log('Connected to server');
        };

        this.socket.onmessage = (event) => {
            //console.log('Server says:', event.data);            
            postMessage(event.data);
        };

        this.socket.onerror = (error) => {
            console.error('WebSocket error:', error);
        };

        this.socket.onclose = () => {
            console.log('Disconnected from server');
        };
    }

    send(data: Float32Array) {
        const buffer = data.buffer as ArrayBuffer;
        this.socket.send(buffer);
    }

    stop() {
        this.socket.send(JSON.stringify({ type: 'stop' }));
    }

    close() {
        this.socket.close();
    }    
}