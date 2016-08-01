/// <reference path="../../typings/index.d.ts" />
import {createStore, combineReducers, applyMiddleware, Dispatch} from "redux";
import thunk from "redux-thunk";
import "whatwg-fetch";

import {reducer as products, IProduct, loadProducts} from "./products";

export type IStoreState = { products: IProduct[] };

export const store = createStore(
    combineReducers({ products }),
    applyMiddleware(thunk));

function fetchProducts(dispatch: Dispatch<IStoreState>) {
    return fetch("/api/products")
        .then(d => d.json())
        .then(prods => dispatch(loadProducts(prods)))
}

store.dispatch(fetchProducts);