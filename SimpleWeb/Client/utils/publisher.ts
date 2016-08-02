import {Action} from "redux-actions";
import {store} from "../store";

interface PublisherStatus {
    success: boolean;
    error: string;
}

class Publisher {
    isOpen: boolean;
    socket: WebSocket;
    loaded: Promise<{}>;
    constructor() {
        this.isOpen = false;
        this.loaded = new Promise((resolve, reject) => {
            this.socket = new WebSocket("ws://localhost:5000");
            this.socket.onopen = () => resolve();
            this.socket.onerror = () => reject("Unable to connect to socket");
            this.socket.onmessage = d => this.handleMessage(JSON.parse(d.data));
        })
    }
    private handleMessage(data: any) {
        console.log("GOT", data);
        store.dispatch(data);
    }
    publish(action: Action<any>) {
        this.loaded.then(() => this.socket.send(JSON.stringify(action)));
    }
}

export default new Publisher();