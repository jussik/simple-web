import {Action} from "redux-actions";

export type ActionProp<T> = (payload: T) => Action<T>;
