import {IResource, Api} from "./resource";

export interface IProduct extends IResource {
    id?: string;
    name: string;
}

export const Products = new Api<IProduct>("products");
