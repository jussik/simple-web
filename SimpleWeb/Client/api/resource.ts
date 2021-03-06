import "whatwg-fetch";

import publisher from "../utils/publisher";

export interface IResource {
    id?: string;
}

export class Api<T extends IResource> {
    path: string;

    constructor(name: string) {
        this.path = "/api/" + name;
    }
    private send(method: string, val: T, id?: string): Promise<Response> {
        return publisher.loaded.then(pub =>
            fetch(this.path + (id ? "/" + id : ""), {
                method: method,
                body: val && JSON.stringify(val),
                headers: new Headers({
                    "Content-Type": "application/json; charset=utf-8",
                    "X-Publisher-Client": pub.id 
                }),
                credentials: "same-origin"
            }));
    }
    one(id: string): Promise<T> {
        return fetch(this.path + "/" + id).then(d => d.json());
    }
    all(): Promise<T[]> {
        return fetch(this.path).then(d => d.json());
    }
    add(val: T): Promise<T> {
        return this.send("POST", val).then(d => d.json());
    }
    update(val: T): Promise<T> {
        return this.send("PUT", val, val.id).then(d => d.json());
    }
    remove(val: T): Promise<void> {
        return this.send("DELETE", undefined, val.id).then(d => {});
    }
}
