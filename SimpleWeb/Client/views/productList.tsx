import * as React from "react";
import {connect} from "react-redux";

import {IStoreState} from "../store";
import {IProduct} from "../api/products";

import Product from "./product";

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
export default connect<IProductListProps, {}, {}>(mapStateToProps)(ProductList);
