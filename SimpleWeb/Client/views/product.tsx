import * as React from "react";

import {IProduct} from "../store/products";

interface IProductProps {
    product: IProduct;
}
export default class Product extends React.Component<IProductProps, {}> {
    render() {
        return <div>{this.props.product.name}</div>;
    }
}