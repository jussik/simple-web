import {handleActions, createAction, Action} from "redux-actions";

export interface IProduct {
    id?: string;
    name: string;
}

export const LOAD = "products/LOAD";

export const loadProducts = createAction<IProduct[]>(LOAD);

export const reducer = handleActions<IProduct[], any>({
    [LOAD]: (state: IProduct[], action: Action<IProduct[]>) => action.payload
}, []);
