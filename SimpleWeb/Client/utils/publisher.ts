import {Action} from "redux-actions";
import {store} from "../store";

interface PublisherStatus {
    open: boolean;
    id?: string;
}

class Publisher {
    socket: WebSocket;
    status: PublisherStatus;
    loaded: Promise<PublisherStatus>;
    constructor() {
        this.status = { open: false };
        this.loaded = new Promise((resolve, reject) => {
            this.socket = new WebSocket("ws://localhost:5000");
            this.socket.onopen = () => {
                resolve(this.status);
                this.socket.onmessage = d => {
                    this.socket.onerror = null;
                    const data = JSON.parse(d.data);
                    this.status.id = data.id;
                    resolve(this.status);
                    this.socket.onmessage = d => this.handleMessage(JSON.parse(d.data));
                }
                this.socket.onclose = () => this.status.open = false;
            }
            this.socket.onerror = () => {
                reject(this.status);
                this.socket.onopen = null;
            }
        });
    }
    private handleMessage(data: any) {
        store.dispatch(data);
    }
    publish(action: Action<any>) {
        this.loaded.then(status => {
            if(!status.open)
                throw new Error("Publisher not connected");
            this.socket.send(JSON.stringify(action))
        });
    }
}

export default new Publisher();