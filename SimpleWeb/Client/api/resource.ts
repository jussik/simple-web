import "whatwg-fetch";

export interface IResource {
    id?: string;
}

export class Api<T extends IResource> {
    path: string;

    constructor(name: string) {
        this.path = "/api/" + name;
    }
    one(id: string): Promise<T> {
        return fetch(this.path + "/" + id)
            .then(d => d.json());
    }
    all(): Promise<T[]> {
        return fetch(this.path)
            .then(d => d.json());
    }
    add(val: T): Promise<T> {
        return fetch(this.path, {
            method: "POST",
            body: JSON.stringify(val)
        }).then(d => d.json());
    }
    update(val: T): Promise<T> {
        return fetch(this.path + "/" + val.id, {
            method: "PUT",
            body: JSON.stringify(val)
        }).then(d => d.json());
    }
    remove(val: T): Promise<void> {
        return fetch(this.path + "/" + val.id, {
            method: "DELETE"
        }).then(d => undefined);
    }
}
