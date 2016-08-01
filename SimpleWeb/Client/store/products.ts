import {handleActions, createAction, Action} from "redux-actions";

import {IProduct} from "../api/products";

export type IProductsState = IProduct[];

export const ADD = "products/ADD";
export const LOAD = "products/LOAD";
export const REMOVE = "products/REMOVE";

export const addProduct = createAction<IProduct>(ADD);
export const loadProducts = createAction<IProduct[]>(LOAD);
export const removeProduct = createAction<IProduct>(REMOVE);

export const reducer = handleActions<IProductsState, any>({
    [ADD]: (state: IProductsState, action: Action<IProduct>) =>
        [...state, action.payload],
    [LOAD]: (state: IProductsState, action: Action<IProduct[]>) =>
        action.payload,
    [REMOVE]: (state: IProductsState, action: Action<IProduct>) =>
        state.filter(p => p.id !== action.payload.id)
}, []);
