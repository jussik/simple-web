/// <reference path="../../typings/index.d.ts" />
import {createStore, combineReducers, applyMiddleware, Dispatch} from "redux";
import {handleActions, createAction, Action} from "redux-actions";
import thunk from "redux-thunk";
import "whatwg-fetch";

export interface IProduct {
    id?: string;
    name: string;
}
export type IStoreState = { products: IProduct[] };

const products = handleActions<IProduct[], any>({
    ADD: (state: IProduct[], action: Action<IProduct>) => [...state, action.payload],
    LOAD: (state: IProduct[], action: Action<IProduct[]>) => action.payload
}, []);

const loadProducts = createAction<IProduct[]>("LOAD");

export const store = createStore(
    combineReducers({ products }),
    applyMiddleware(thunk));

function fetchProducts(dispatch: Dispatch<IStoreState>) {
    return fetch("/api/products")
        .then(d => d.json())
        .then(prods => dispatch(loadProducts(prods)))
}

store.dispatch(fetchProducts);