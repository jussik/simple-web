import * as ReactDOM from "react-dom";
import * as React from "react";
import {Provider} from "react-redux";

import {store} from "../store";

import ProductList from "./productList";

ReactDOM.render(<Provider store={store}>
    <ProductList/>
</Provider>, document.getElementById("main"));