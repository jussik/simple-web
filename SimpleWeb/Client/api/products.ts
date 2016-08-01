import {IResource, Api} from "./resource";

export interface IProduct extends IResource {
    name: string;
}

export const Products = new Api<IProduct>("products");
