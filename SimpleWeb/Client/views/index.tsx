/// <reference path="../../typings/index.d.ts" />
import * as ReactDOM from "react-dom";
import * as React from "react";
import {Provider, connect} from "react-redux";

import {store, IStoreState, IProduct} from "../store";

interface IProductProps {
    product: IProduct;
}
class Product extends React.Component<IProductProps, {}> {
    render() {
        return <div>{this.props.product.name}</div>;
    }
}

interface IProductListProps {
    products: IProduct[];
}
class ProductList extends React.Component<IProductListProps, {}> {
    render() {
        const prods = this.props.products
        return <div>
            <h3>Products!</h3>
            {this.props.products.map(p => (
                <Product product={p} key={p.id}></Product>
            ))}
        </div>;
    }
}
const mapStateToProps = (state: IStoreState) => ({
    products: state.products
});
const ProductListRedux = connect<IProductListProps, {}, {}>(mapStateToProps)(ProductList);

ReactDOM.render(<Provider store={store}>
    <ProductListRedux></ProductListRedux>
</Provider>, document.getElementById("main"));