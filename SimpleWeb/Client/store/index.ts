import {createStore, combineReducers, applyMiddleware, Dispatch} from "redux";
import thunk from "redux-thunk";
import "whatwg-fetch";

import {reducer as products, loadProducts, IProductsState} from "./products";
import {Products} from "../api/products";

export type IStoreState = { products: IProductsState };

export const store = createStore(
    combineReducers({ products }),
    applyMiddleware(thunk));

function fetchProducts(dispatch: Dispatch<IStoreState>) {
    return Products.all().then(prods => dispatch(loadProducts(prods)));
}

store.dispatch(fetchProducts);
